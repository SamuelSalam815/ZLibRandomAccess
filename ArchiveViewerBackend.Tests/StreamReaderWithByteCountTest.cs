using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(StreamReaderWithByteCount))]
public class StreamReaderWithByteCountTest
{
    private static readonly Encoding TestEncoding = Encoding.UTF8;

    public static IEnumerable<object?[]> PeekTestDataEnumerable()
    {
        yield return ["foo bar", 'f'];
        yield return ["Hello, World!", 'H'];
    }

    [TestMethod]
    [DynamicData(nameof(PeekTestDataEnumerable))]
    public void Peek_ReturnsNextCharacter(string input, char nextCharacter)
    {
        new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding)
            .Peek()
            .ShouldBe(nextCharacter);
    }

    [TestMethod]
    [DynamicData(nameof(PeekTestDataEnumerable))]
    public void Peek_IsIdempotent(string input, char nextCharacter)
    {
        var stream = new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding);
        _ = stream.Peek();
        stream.Peek().ShouldBe(nextCharacter);
    }

    [TestMethod]
    [DataRow(SampleTexts.FizzBuzz20)]
    [DataRow(SampleTexts.PeanutButterJellyLyrics)]
    [DataRow(SampleTexts.MixedNewLineStyles)]
    public void Read_ReturnsExpectedString(string input)
    {
        new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding)
            .ReadToEnd()
            .ShouldBe(input);
    }

    [TestMethod]
    public void NumberOfBytesAndCharactersRead_IsReportedCorrectlyWhenReadingLineByLine()
    {
        using var sut = new  StreamReaderWithByteCount(EncodeWith(SampleTexts.MixedNewLineStyles, TestEncoding), TestEncoding);

        while (sut.ReadLine() is not null) ;

        sut.NumberOfBytesRead.ShouldBe(TestEncoding.GetByteCount(SampleTexts.MixedNewLineStyles));
        sut.NumberOfCharactersRead.ShouldBe(SampleTexts.MixedNewLineStyles.Length);
    }

    [TestMethod]
    [DataRow("\r")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ByteCount_CannotBeAccuratelyDeterminedBtReadingLineByLine(string guessedLineEnding)
    {
        var encoding = TestEncoding;
        const string inputText = SampleTexts.MixedNewLineStyles;
        using var sut = new  StreamReaderWithByteCount(EncodeWith(inputText, encoding), encoding);
        var estimatedLineEndingByteCount = encoding.GetByteCount(guessedLineEnding);
        var estimatedByteCount = 0;

        while (sut.ReadLine() is {} line)
        {
            estimatedByteCount += encoding.GetByteCount(line);
            estimatedByteCount += estimatedLineEndingByteCount;
        }

        estimatedByteCount.ShouldNotBe(encoding.GetByteCount(inputText));
    }

    private static Stream EncodeWith(string input, Encoding testEncoding)
    {
        return new MemoryStream(testEncoding.GetBytes(input));
    }

    [TestMethod]
    public void LeaveOpen_DoesNotDisposeInnerStream()
    {
        var textStream = EncodeWith("test", TestEncoding);
        var sut = new StreamReaderWithByteCount(textStream, TestEncoding, leaveOpen: true);
        sut.Dispose();
        var action = textStream.ReadByte;
        action.ShouldNotThrow();
    }

    [TestMethod]
    public void Dispose_DisposesInnerStream()
    {
        var textStream = EncodeWith("test", TestEncoding);
        var sut = new StreamReaderWithByteCount(textStream, TestEncoding);
        sut.Dispose();
        Action action = () => textStream.ReadByte();
        action.ShouldThrow<Exception>();
    }
}
