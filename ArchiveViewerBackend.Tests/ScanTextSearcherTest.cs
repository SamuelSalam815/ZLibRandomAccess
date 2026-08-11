using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(ScanTextSearcher))]
public class ScanTextSearcherTest
{
    [TestMethod]
    [DataRow("pbj", SampleTexts.PeanutButterJellyLyrics)]
    public void ReturnNull_WhenPatternIsNotFound(string searchPattern, string text)
    {
        CreateScanTextSearcher(text).FindNext(new Regex(searchPattern)).ShouldBeNull();
    }

    [TestMethod]
    [DynamicData(nameof(FirstOccurrenceTestCases))]
    public void ReturnCorrectByteOffset_WhenPatternIsFound(string searchPattern, int expectedByteOffset, string text)
    {
        CreateScanTextSearcher(text)
            .FindNext(new Regex(searchPattern))
            .ShouldNotBeNull()
            .ByteOffset.ShouldBe(expectedByteOffset);
    }

    [TestMethod]
    [DynamicData(nameof(AllOccurrencesTestCases))]
    public void ReturnCorrectByteOffsets_WhenAllOccurrencesOfPatternIsFound(string searchPattern, int[] expectedByteOffsets, string text)
    {
        CreateScanTextSearcher(text)
            .FindAll(new Regex(searchPattern))
            .Select(x => x.ByteOffset)
            .ToList()
            .ShouldBe(expectedByteOffsets);
    }

    public static IEnumerable<object[]> FirstOccurrenceTestCases()
    {
        yield return CreateFirstOccurrenceTestCase("Fizz", 6, SampleTexts.FizzBuzz20);
        yield return CreateFirstOccurrenceTestCase("Buzz", 15, SampleTexts.FizzBuzz20);
        yield return CreateFirstOccurrenceTestCase("4", 12, SampleTexts.FizzBuzz20);
        yield return CreateFirstOccurrenceTestCase(@"\d", 0, SampleTexts.FizzBuzz20);
    }

    public static IEnumerable<object?[]> AllOccurrencesTestCases()
    {
        yield return CreateAllOccurrencesTestCase("z", [6,7,13,14], "1\n2\nFizz\n4\nBuzz");
    }

    private static ScanTextSearcher CreateScanTextSearcher(string text)
    {
        return new ScanTextSearcher(new MemoryStream(Encoding.UTF8.GetBytes(text)));
    }

    private static object[] CreateFirstOccurrenceTestCase(
        [StringSyntax(StringSyntaxAttribute.Regex)] string searchPattern,
        int expectedByteOffset,
        string text)
    {
        return [searchPattern, expectedByteOffset, text];
    }

    private static object[] CreateAllOccurrencesTestCase(
        [StringSyntax(StringSyntaxAttribute.Regex)] string searchPattern,
        int[] expectedByteOffsets,
        string text)
    {
        return [searchPattern, expectedByteOffsets, text];
    }
}
