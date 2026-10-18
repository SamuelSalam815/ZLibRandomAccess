using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(LineAwareReader))]
public class LineAwareReaderTest
{
    public static IEnumerable<object[]> Encodings()
    {
        yield return [Encoding.UTF8];
        yield return [Encoding.Unicode];
        yield return [Encoding.UTF32];
        yield return [Encoding.Latin1];
    }

    private static LineAwareReader CreateReader(
        string text,
        Encoding encoding,
        int targetLineCount,
        int capacityInChars,
        long lineAtStart = 0,
        bool startsMidLine = false) =>
        new(new MemoryStream(encoding.GetBytes(text)), encoding, targetLineCount, capacityInChars, lineAtStart, startsMidLine);

    private static string TextOf(LineAwareReader reader)
    {
        var chars = new char[reader.Text.Length];
        reader.Text.CopyTo(chars);
        return new string(chars);
    }

    [TestMethod]
    [DynamicData(nameof(Encodings))]
    public void Fill_ReadsUpToTheTargetNumberOfLines(Encoding encoding)
    {
        var sut = CreateReader("a\nb\nc\nd\n", encoding, targetLineCount: 2, capacityInChars: 100);

        sut.Fill();

        sut.LineCount.ShouldBeGreaterThanOrEqualTo(2);
        TextOf(sut).ShouldStartWith("a\nb\n");
        sut.FirstLineNumber.ShouldBe(0);
    }

    [TestMethod]
    public void Fill_WhenTheStreamEndsEarly_CountsTheUnfinishedLastLine()
    {
        var sut = CreateReader("a\nb", Encoding.UTF8, targetLineCount: 5, capacityInChars: 100);

        sut.Fill();

        sut.IsAtEndOfStream.ShouldBeTrue();
        sut.LineCount.ShouldBe(2);
        TextOf(sut).ShouldBe("a\nb");
    }

    [TestMethod]
    public void Fill_WhenALineIsLongerThanTheBuffer_Throws()
    {
        var sut = CreateReader(new string('x', 100) + "\n", Encoding.UTF8, targetLineCount: 1, capacityInChars: 10);

        Should.Throw<LineTooLongException>(() => sut.Fill());
    }

    [TestMethod]
    public void Fill_KeepsCharactersSplitAcrossReadsIntact()
    {
        // With 7 characters of capacity each read is 6 bytes or fewer, so UTF-8 pairs of bytes get split.
        var sut = CreateReader("éééé\nééé\n", Encoding.UTF8, targetLineCount: 1, capacityInChars: 7);

        sut.Fill();

        TextOf(sut).ShouldBe("éééé\n");
    }

    [TestMethod]
    [DynamicData(nameof(Encodings))]
    public void DropLinesThenFill_KeepsTheTargetNumberOfLines(Encoding encoding)
    {
        var sut = CreateReader("l0\nl1\nl2\nl3\nl4\nl5\n", encoding, targetLineCount: 2, capacityInChars: 12);
        sut.Fill();

        sut.DropLines(sut.LineCount - 1);
        sut.Fill();

        sut.LineCount.ShouldBeGreaterThanOrEqualTo(2);
        sut.FirstLineNumber.ShouldBeGreaterThan(0);
        TextOf(sut).ShouldStartWith($"l{sut.FirstLineNumber}\n");
    }

    [TestMethod]
    public void DropLines_RemovesWholeLinesAndAdvancesTheFirstLineNumber()
    {
        var sut = CreateReader("l0\nl1\nl2\n", Encoding.UTF8, targetLineCount: 3, capacityInChars: 100);
        sut.Fill();

        sut.DropLines(2);

        TextOf(sut).ShouldBe("l2\n");
        sut.FirstLineNumber.ShouldBe(2);
        sut.LineCount.ShouldBe(1);
    }

    [TestMethod]
    public void DropLines_CanDropAnUnfinishedLastLine()
    {
        var sut = CreateReader("l0\nl1", Encoding.UTF8, targetLineCount: 3, capacityInChars: 100);
        sut.Fill();

        sut.DropLines(2);

        TextOf(sut).ShouldBe("");
        sut.LineCount.ShouldBe(0);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(4)]
    public void DropLines_OutOfRange_Throws(int count)
    {
        var sut = CreateReader("l0\nl1\nl2\n", Encoding.UTF8, targetLineCount: 3, capacityInChars: 100);
        sut.Fill();

        Should.Throw<ArgumentOutOfRangeException>(() => sut.DropLines(count));
    }

    [TestMethod]
    public void Fill_WhenStartingMidLine_DiscardsThePartialLine()
    {
        var sut = CreateReader("tail\nfull1\nfull2\n", Encoding.UTF8, targetLineCount: 2, capacityInChars: 100, lineAtStart: 7, startsMidLine: true);

        sut.Fill();

        TextOf(sut).ShouldBe("full1\nfull2\n");
        sut.FirstLineNumber.ShouldBe(8);
    }

    [TestMethod]
    public void Fill_WhenThePartialLineIsLongerThanTheBuffer_DoesNotThrow()
    {
        var sut = CreateReader(new string('t', 100) + "\nfull\n", Encoding.UTF8, targetLineCount: 1, capacityInChars: 10, startsMidLine: true);

        sut.Fill();

        TextOf(sut).ShouldBe("full\n");
        sut.FirstLineNumber.ShouldBe(1);
    }

    [TestMethod]
    public void Constructor_CapacityBelowTwo_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => CreateReader("a", Encoding.UTF8, 1, 1));
    }
}
