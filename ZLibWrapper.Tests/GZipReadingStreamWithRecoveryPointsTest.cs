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
}
