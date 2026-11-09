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
    [DataRow(SampleTexts.FizzBuzz20)]
    public void ByteCount_OnlyIncreasesAsBytesAreRead(string input)
    {
        var sut = new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding);

        var previousByteCount = sut.NumberOfBytesRead;

        while (sut.ReadLine() is not null)
        {
            var currentByteCount = sut.NumberOfBytesRead;
            currentByteCount.ShouldBeGreaterThan(previousByteCount);
            previousByteCount = currentByteCount;
        }
    }

    [TestMethod]
    public void NumberOfBytesAndCharactersRead_IsReportedCorrectlyWhenReadingLineByLine()
    {
        using var sut = new  StreamReaderWithByteCount(EncodeWith(SampleTexts.MixedNewLineStyles, TestEncoding), TestEncoding);

        while (sut.ReadLine() is not null) ;

        sut.NumberOfBytesRead.ShouldBe(TestEncoding.GetByteCount(SampleTexts.MixedNewLineStyles));
    }

    [TestMethod]
    [DataRow("UTF-8")]
    [DataRow("utf-16")]
    [DataRow("utf-32")]
    public void NumberOfBytesRead_CountsASurrogatePairOnceWithItsEncodedSize(string encodingName)
    {
        var encoding = Encoding.GetEncoding(encodingName);
        using var sut = new StreamReaderWithByteCount(EncodeWith("a😀b", encoding), encoding);
        var bytesAfterEachRead = new List<int>();

        for (var character = sut.Read(); character != -1; character = sut.Read())
        {
            bytesAfterEachRead.Add(sut.NumberOfBytesRead);
        }

        // The reads return "a", the two halves of the emoji, and "b".
        bytesAfterEachRead.ShouldBe([
            encoding.GetByteCount("a"),
            encoding.GetByteCount("a"),
            encoding.GetByteCount("a😀"),
            encoding.GetByteCount("a😀b"),
        ]);
    }

    [TestMethod]
    public void Constructor_BufferTooSmallForOneCharacter_Throws()
    {
        // A surrogate pair can take two characters of room in UTF-8.
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new StreamReaderWithByteCount(EncodeWith("a", Encoding.UTF8), Encoding.UTF8, bufferSize: 1));
    }

    [TestMethod]
    public void Read_SingleByteEncoding_WorksWithAOneCharacterBuffer()
    {
        // Latin-1 never needs two characters of room for one character.
        using var sut = new StreamReaderWithByteCount(EncodeWith("abc", Encoding.Latin1), Encoding.Latin1, bufferSize: 1);

        sut.ReadToEnd().ShouldBe("abc");
    }

    [TestMethod]
    public void ResetByteCount_SetsByteCountToZero()
    {
        using var sut = new  StreamReaderWithByteCount(EncodeWith(SampleTexts.MixedNewLineStyles, TestEncoding), TestEncoding);

        while (sut.ReadLine() is not null) ;
        sut.ResetByteCount();

        sut.NumberOfBytesRead.ShouldBe(0);
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
    public void Peek_OnEmptyStream_ReturnsMinusOne()
    {
        using var sut = new StreamReaderWithByteCount(EncodeWith("", TestEncoding), TestEncoding);

        sut.Peek().ShouldBe(-1);
    }

    [TestMethod]
    public void Peek_AfterReadingEveryCharacter_ReturnsMinusOne()
    {
        using var sut = new StreamReaderWithByteCount(EncodeWith("ab", TestEncoding), TestEncoding);
        sut.Read().ShouldBe('a');
        sut.Read().ShouldBe('b');

        sut.Peek().ShouldBe(-1);
    }

    [TestMethod]
    [DataRow("UTF-8")]
    [DataRow("utf-16")]
    public void Read_CharactersThatNeedTwoCodeUnits_AreDecodedWhateverThePositionInTheBuffer(string encodingName)
    {
        // With a buffer of 4 characters, the three letters leave the next write position at the last cell,
        // where a surrogate pair does not fit.
        var encoding = Encoding.GetEncoding(encodingName);
        const string text = "abc\U0001F600\U0001F600";
        using var sut = new StreamReaderWithByteCount(EncodeWith(text, encoding), encoding, bufferSize: 4);

        sut.ReadToEnd().ShouldBe(text);
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
