using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    private static object[] CreateFirstOccurrenceTestCase(
        [StringSyntax(StringSyntaxAttribute.Regex)] string searchPattern,
        long expectedByteOffset,
        string text)
    {
        return [searchPattern, expectedByteOffset, text];
    }

    public static IEnumerable<object[]> FirstOccurrenceTestCases()
    {
        // yield return CreateFirstOccurrenceTestCase("Fizz", 4, SampleTexts.FizzBuzz20);
        // yield return CreateFirstOccurrenceTestCase("Buzz", 15, SampleTexts.FizzBuzz20);
        // yield return CreateFirstOccurrenceTestCase("4", 12, SampleTexts.FizzBuzz20);
        yield return CreateFirstOccurrenceTestCase(@"\d", 0, SampleTexts.FizzBuzz20);
    }

    [TestMethod]
    [DynamicData(nameof(FirstOccurrenceTestCases))]
    public void ReturnCorrectByteOffset_WhenPatternIsFound(string searchPattern, long expectedByteOffset, string text)
    {
        // todo consider if this is duplicated test
        CreateScanTextSearcher(text)
            .FindNext(new Regex(searchPattern))
            .ShouldNotBeNull()
            .Offset.ShouldBe(expectedByteOffset);
    }

    private ScanTextSearcher CreateScanTextSearcher(string text)
    {
        return new ScanTextSearcher(new MemoryStream(Encoding.UTF8.GetBytes(text)));
    }
}
