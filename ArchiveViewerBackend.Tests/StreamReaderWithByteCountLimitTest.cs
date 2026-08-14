using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(StreamReaderWithByteCountLimit))]
public class StreamReaderWithByteCountLimitTest
{
    [TestMethod]
    [DynamicData(nameof(ByteLimitTestCases))]
    public void StreamReaderEnds_WhenMaximumNumberOfBytesIsRead(
        int? byteLimit,
        int expectedFinalByteCount,
        string inputText
        )
    {
        var encoding = Encoding.Default;
        using var inputStream = new MemoryStream(encoding.GetBytes(inputText));
        using var sut = new StreamReaderWithByteCountLimit(inputStream, encoding, maximumNumberOfBytesRead: byteLimit);
        _ = sut.ReadToEnd();
        sut.NumberOfBytesRead.ShouldBe(expectedFinalByteCount);
    }

    public static IEnumerable<object?[]> ByteLimitTestCases()
    {
        yield return [null, 3290, SampleTexts.PeanutButterJellyLyrics];
        yield return [4000, 3290, SampleTexts.PeanutButterJellyLyrics];
        yield return [5, 5, SampleTexts.PeanutButterJellyLyrics];
        yield return [200, 200, SampleTexts.PeanutButterJellyLyrics];
    }
}
