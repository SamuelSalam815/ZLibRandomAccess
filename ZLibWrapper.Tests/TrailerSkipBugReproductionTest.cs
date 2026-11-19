using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;

namespace ZLibWrapper.Tests;

/// <summary>
/// Reproduces the trailer-skip bug fixed in commit c6317d8.
/// Phase A (ClassInitialize) sweeps stream lengths to find compressed files that
/// pass and fail under the broken 4-byte seek, then caches them for hypothesis tests.
/// </summary>
[TestClass]
public class TrailerSkipBugReproductionTest
{
    private record TestCase(
        byte[] CompressedBytes,
        List<RecoveryPointOffset> RecoveryPoints,
        byte[] OriginalData,
        int StreamLength,
        bool FailsWithBrokenSeek);

    private static List<TestCase> _successCases = [];
    private static List<TestCase> _failureCases = [];

    private const ZWindowBits GZipWindowBits = ZWindowBits.DefaultWindowSize | ZWindowBits.GZipStream;

    [ClassInitialize]
    public static void CaptureTestCases(TestContext _)
    {
        var successes = new List<TestCase>();
        var failures = new List<TestCase>();

        for (var length = 4000; length < 10000 && (successes.Count < 3 || failures.Count < 1); length++)
        {
            var data = new byte[length];
            new Random(length).NextBytes(data);
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)('a' + data[i] % 8);

            var compressed = new MemoryStream();
            var recoveryPoints = new List<RecoveryPointOffset>();
            using (var writer = new GZipWritingStreamWithRecoveryPoints(compressed, true, 4000))
            {
                writer.RecoveryPointWritten += recoveryPoints.Add;
                for (var offset = 0; offset < data.Length; offset += 100)
                    writer.Write(data, offset, Math.Min(100, data.Length - offset));
            }

            var failed = !TryReadAfterJump(compressed.ToArray(), recoveryPoints, seekBytes: 4);
            var tc = new TestCase(compressed.ToArray(), recoveryPoints, data, length, failed);

            if (failed)
                failures.Add(tc);
            else
                successes.Add(tc);
        }

        successes.Count.ShouldBeGreaterThanOrEqualTo(3, "need at least 3 success cases");
        failures.Count.ShouldBeGreaterThanOrEqualTo(1, "need at least 1 failure case");

        _successCases = successes.Take(3).ToList();
        _failureCases = failures;
    }

    [TestMethod]
    public void Seek8_PassesAll()
    {
        foreach (var tc in AllCases())
            TryReadAfterJump(tc.CompressedBytes, tc.RecoveryPoints, seekBytes: 8)
                .ShouldBeTrue($"length {tc.StreamLength} should pass with seek=8");
    }

    [TestMethod]
    public void SweepSeekValues_FailureCasesHaveThreshold()
    {
        foreach (var tc in _failureCases)
        {
            int? threshold = null;
            for (var seek = 1; seek <= 8; seek++)
            {
                var passed = TryReadAfterJump(tc.CompressedBytes, tc.RecoveryPoints, seekBytes: seek);
                if (passed && threshold is null)
                    threshold = seek;
                if (threshold is not null)
                    passed.ShouldBeTrue(
                        $"length {tc.StreamLength}: seek={seek} should pass once threshold ({threshold}) is reached");
            }

            threshold.ShouldNotBeNull($"length {tc.StreamLength}: no seek value 1-8 succeeded");
            threshold.Value.ShouldBeGreaterThan(0);
            threshold.Value.ShouldBeLessThanOrEqualTo(8);
        }
    }

    [TestMethod]
    public void Seek0_PassesNormalCases()
    {
        foreach (var tc in _successCases)
            TryReadAfterJump(tc.CompressedBytes, tc.RecoveryPoints, seekBytes: 0)
                .ShouldBeTrue($"length {tc.StreamLength}: seek=0 should pass when trailer was fully buffered");
    }

    [TestMethod]
    public void ProductionFix_PassesAll()
    {
        foreach (var tc in AllCases())
            TryReadAfterJump(tc.CompressedBytes, tc.RecoveryPoints, seekBytes: 4, useProductionFix: true)
                .ShouldBeTrue($"length {tc.StreamLength} should pass with production fix");
    }

    private static bool TryReadAfterJump(
        byte[] compressedBytes,
        List<RecoveryPointOffset> recoveryPoints,
        int seekBytes,
        bool useProductionFix = false)
    {
        try
        {
            using var compressed = new MemoryStream(compressedBytes);
            using var reader = new ZLibInflateStreamWithRecoveryPoints(compressed, windowBits: GZipWindowBits)
            {
                TrailerSeekBytes = seekBytes,
                UseProductionFix = useProductionFix
            };
            var last = recoveryPoints[^1];
            reader.JumpTo(last.OffsetInCompressedStream);
            using var output = new MemoryStream();
            reader.CopyTo(output);
            return true;
        }
        catch (ZLibException)
        {
            return false;
        }
    }

    private IEnumerable<TestCase> AllCases() => _successCases.Concat(_failureCases);
}
