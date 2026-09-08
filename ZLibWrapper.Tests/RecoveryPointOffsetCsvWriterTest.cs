using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ZLibWrapper.Tests;

[TestClass]
[TestSubject(typeof(RecoveryPointOffsetCsvWriter))]
public class RecoveryPointOffsetCsvWriterTest
{

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void StreamIsClosed_WhenDisposedAndLeaveOpenFalse(bool leaveOpen)
    {
        var stream = new MemoryStream();
        var writeOperation = () => stream.Write([2]);

        writeOperation.ShouldNotThrow();

        new RecoveryPointOffsetCsvWriter(stream, Encoding.Default, leaveOpen: leaveOpen).Dispose();

        if (leaveOpen)
        {
            writeOperation.ShouldNotThrow();
        }
        else
        {
            writeOperation.ShouldThrow<Exception>();
        }
    }

    public static IEnumerable<object?[]> Encodings()
    {
        yield return [Encoding.Default];
        yield return [Encoding.ASCII];
        yield return [Encoding.Unicode];
        yield return [Encoding.UTF8];
    }

    [TestMethod]
    [DynamicData(nameof(Encodings))]
    public void WritingUsingCsvHelper_ProducesExpectedResult(Encoding encoding)
    {
        var records = new List<RecoveryPointOffset> { new(1,2), new(3,4), };
        var expectedResult =
            $"""
             {nameof(RecoveryPointOffset.OffsetInUncompressedStream)},{nameof(RecoveryPointOffset.OffsetInCompressedStream)}
             1,2
             3,4

             """;
        using var memoryStream = new MemoryStream();
        using (var sut = new RecoveryPointOffsetCsvWriter(memoryStream, encoding, leaveOpen: true))
        {
            sut.WriteAll(records);
        }
        memoryStream.Position = 0;
        using var streamReader = new StreamReader(memoryStream, encoding);
        var actualResult = streamReader.ReadToEnd();

        actualResult.ShouldBe(expectedResult);
    }
}
