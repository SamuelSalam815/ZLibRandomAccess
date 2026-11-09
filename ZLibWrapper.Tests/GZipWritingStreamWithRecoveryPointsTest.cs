using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ZLibWrapper.Tests;

[TestClass]
[TestSubject(typeof(GZipWritingStreamWithRecoveryPoints))]
public class GZipWritingStreamWithRecoveryPointsTest
{
    private static List<RecoveryPointOffset> WriteChunks(Encoding encoding, params byte[][] chunks)
    {
        var recoveryPoints = new List<RecoveryPointOffset>();
        // An interval of 1 byte puts a recovery point after every write.
        using var sut = new GZipWritingStreamWithRecoveryPoints(
            new MemoryStream(), recoveryPointByteInterval: 1, encoding: encoding);
        sut.RecoveryPointWritten += recoveryPoints.Add;
        foreach (var chunk in chunks)
        {
            sut.Write(chunk);
        }

        return recoveryPoints;
    }

    private static RecoveryPointOffset Strip(RecoveryPointOffset point) => point with { OffsetInCompressedStream = 0 };

    [TestMethod]
    public void RecoveryPoints_CountLines_ForUtf8()
    {
        var points = WriteChunks(
            Encoding.UTF8,
            Encoding.UTF8.GetBytes("ab\nc"),
            Encoding.UTF8.GetBytes("d\n"),
            Encoding.UTF8.GetBytes("e"));

        points.ConvertAll(Strip).ShouldBe([
            new RecoveryPointOffset(4, 0, 1),
            new RecoveryPointOffset(6, 0, 2),
            new RecoveryPointOffset(7, 0, 2),
        ]);
    }

    [TestMethod]
    public void RecoveryPoints_CountLines_WhenUtf16CharacterIsSplitAcrossWrites()
    {
        // "a\nb" in UTF-16LE is 61 00 0A 00 62 00. The first write ends inside the line feed.
        var bytes = Encoding.Unicode.GetBytes("a\nb");

        var points = WriteChunks(Encoding.Unicode, bytes[..3], bytes[3..]);

        points.ConvertAll(Strip).ShouldBe([
            new RecoveryPointOffset(3, 0, 0),
            new RecoveryPointOffset(6, 0, 1),
        ]);
    }
}
