using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ZLibWrapper.Tests;

[TestClass]
[TestSubject(typeof(GZipReadingStreamWithRecoveryPoints))]
public class GZipReadingStreamWithRecoveryPointsTest
{

    public static IEnumerable<object[]> RecoverPointJumpTestCases()
    {
        byte[][] recoverableSegments =
        [
            [1, 2, 3],
            [4, 5, 6],
            [7, 8, 9],
            [10, 11, 12],
        ];
        byte[] expectedStream = [7, 8, 9, 10, 11, 12];
        yield return [recoverableSegments, 1, 6L, expectedStream];
    }

    [TestMethod]
    [DynamicData(nameof(RecoverPointJumpTestCases))]
    public void JumpingToARecoveryPoint_ShouldAllowReadingOfBytesAfterRecoveryPoint(
        byte[][] recoverySegments,
        int recoveryPointToJumpTo,
        long expectedUncompressedOffset,
        byte[] expectedBytesRead
        )
    {
        var memoryStream = new MemoryStream();
        var recoverySegmentLength = recoverySegments[0].Length;
        var recoveryPoints = new List<RecoveryPointOffset>();
        using (var writer = new GZipWritingStreamWithRecoveryPoints(
                   memoryStream,
                   true,
                   recoverySegmentLength))
        {
            writer.RecoveryPointWritten += recoveryPoints.Add;
            foreach (var segment in recoverySegments)
            {
                writer.Write(segment);
            }
        }

        memoryStream.Position = 0;
        using var reader = new GZipReadingStreamWithRecoveryPoints(memoryStream, recoveryPoints);
        reader.JumpTo(recoveryPointToJumpTo).OffsetInUncompressedStream.ShouldBe(expectedUncompressedOffset);

        var actualBytes = new byte[expectedBytesRead.Length];
        reader.ReadExactly(actualBytes);

        actualBytes.ShouldBe(expectedBytesRead);
    }

    [TestMethod]
    public void ReadingToTheEndAfterJumpingToTheLastRecoveryPoint_ShouldNotFailForAnyStreamLength()
    {
        // The reader fills its input buffer in 1024 byte pieces. Sweep the stream length so the end of the
        // compressed data lands on every position of a piece, including just before the 8 byte gzip trailer.
        const int inputBufferSize = 1024;
        const int gzipTrailerSize = 8;
        var failures = new List<string>();
        var sawEndOfDataAtEndOfBuffer = false;
        for (var length = 4000; length < 4000 + 6000; length++)
        {
            var data = new byte[length];
            new System.Random(length).NextBytes(data);
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)('a' + data[i] % 8);
            }

            var compressed = new MemoryStream();
            var recoveryPoints = new List<RecoveryPointOffset>();
            using (var writer = new GZipWritingStreamWithRecoveryPoints(compressed, true, 4000))
            {
                writer.RecoveryPointWritten += recoveryPoints.Add;
                // A recovery point is only considered after each write, so write in small pieces.
                for (var offset = 0; offset < data.Length; offset += 100)
                {
                    writer.Write(data, offset, Math.Min(100, data.Length - offset));
                }
            }

            compressed.Position = 0;
            using var reader = new GZipReadingStreamWithRecoveryPoints(compressed, recoveryPoints);
            var last = reader.JumpTo(recoveryPoints.Count - 1);
            sawEndOfDataAtEndOfBuffer |=
                (compressed.Length - last.OffsetInCompressedStream) % inputBufferSize == gzipTrailerSize;
            try
            {
                var actual = new MemoryStream();
                reader.CopyTo(actual);
                actual.ToArray().ShouldBe(data[(int)last.OffsetInUncompressedStream..]);
            }
            catch (ZLibException e)
            {
                failures.Add($"length {length}: {e.Message}");
            }
        }

        sawEndOfDataAtEndOfBuffer.ShouldBeTrue("the sweep never exercised the case under test");
        failures.ShouldBeEmpty();
    }
}
