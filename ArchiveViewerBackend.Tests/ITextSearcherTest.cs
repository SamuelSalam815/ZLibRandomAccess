using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(ScanTextSearcher))]
[TestSubject(typeof(ParallelScanTextSearcher))]
public class TextSearcherTest
{
    [TestMethod]
    [DynamicData(nameof(PatternNotFoundTestCases))]
    public void FindNextReturnNull_WhenPatternIsNotFound(ITextSearcher sut, string searchPattern)
    {
        sut.FindNext(searchPattern).ShouldBeNull();
    }

    [TestMethod]
    [DynamicData(nameof(PatternNotFoundTestCases))]
    public void FindAllReturnsEmpty_WhenPatternIsNotFound(ITextSearcher sut, string searchPattern)
    {
        sut.FindAll(searchPattern).ShouldBeEmpty();
    }

    [TestMethod]
    [DynamicData(nameof(FirstOccurrenceTestCases))]
    public void FindNextReturnsCorrectByteOffset_WhenPatternIsFound(
        ITextSearcher sut,
        string searchPattern,
        int expectedByteOffset)
    {
        sut.FindNext(searchPattern).ShouldNotBeNull().ByteOffset.ShouldBe(expectedByteOffset);
    }

    [TestMethod]
    [DynamicData(nameof(AllOccurrencesTestCases))]
    public void FindNextReturnsCorrectByteOffsetsInOrder_WhenPatternIsFound(
        ITextSearcher sut,
        string searchPattern,
        int[] expectedByteOffsets)
    {
        foreach (var expectedByteOffset in expectedByteOffsets)
        {
            sut.FindNext(searchPattern).ShouldNotBeNull().ByteOffset.ShouldBe(expectedByteOffset);
        }

        sut.FindNext(searchPattern).ShouldBeNull();
    }

    [TestMethod]
    [DynamicData(nameof(AllOccurrencesTestCases))]
    public void FindAllReturnsCorrectByteOffsets_WhenPatternIsFound(
        ITextSearcher sut,
        string searchPattern,
        int[] expectedByteOffsets)
    {
        sut.FindAll(searchPattern).Select(x => x.ByteOffset).ToList().ShouldBe(expectedByteOffsets);
    }

    [TestMethod]
    [DynamicData(nameof(AllOccurrencesTestCases))]
    public void FindNextReturnsNull_AfterFindAll(
        ITextSearcher sut,
        string searchPattern,
        int[] expectedByteOffsets)
    {
        sut.FindAll(searchPattern).Select(x => x.ByteOffset).ToList().ShouldBe(expectedByteOffsets);
        sut.FindNext(searchPattern).ShouldBeNull();
    }

    [TestMethod]
    [DynamicData(nameof(MatchTextTestCases))]
    public void FindAllReturnsExpectedMatchText_WhenPatternIsFound(
        ITextSearcher sut,
        string searchPattern,
        string[] expectedMatchTexts)
    {
        sut.FindAll(searchPattern).Select(result => result.MatchText).ShouldBe(expectedMatchTexts);
    }

    [TestMethod]
    [DynamicData(nameof(MatchTextTestCases))]
    public void FindNextReturnsExpectedMatchText_WhenPatternIsFound(
        ITextSearcher sut,
        string searchPattern,
        string[] expectedMatchTexts)
    {
        foreach (var expectedMatchText in expectedMatchTexts)
        {
            sut.FindNext(searchPattern).ShouldNotBeNull().MatchText.ShouldBe(expectedMatchText);
        }

        sut.FindNext(searchPattern).ShouldBeNull();
    }

    public static IEnumerable<object?[]> MatchTextTestCases()
    {
        return WithAllTextSearcherVariants(
            SampleTexts.FizzBuzz20,
            [@"\d", new[]
            {
                "1",
                "2",
                "4",
                "7",
                "1","1",
                "1","3",
                "1","4",
                "1","6",
                "1","7",
                "1","9"
            }]
        );
    }

    public static IEnumerable<object?[]> PatternNotFoundTestCases()
    {
        return WithAllTextSearcherVariants(SampleTexts.PeanutButterJellyLyrics, ["pbj"]);
    }

    public static IEnumerable<object[]> FirstOccurrenceTestCases()
    {
        return WithAllTextSearcherVariants(SampleTexts.FizzBuzz20,
        CreateFirstOccurrenceTestCase("Fizz", 6),
        CreateFirstOccurrenceTestCase("Buzz", 15),
        CreateFirstOccurrenceTestCase("4", 12),
        CreateFirstOccurrenceTestCase(@"\d", 0));
    }

    public static IEnumerable<object?[]> AllOccurrencesTestCases()
    {
        return WithAllTextSearcherVariants("1\n2\nFizz\n4\nBuzz", CreateAllOccurrencesTestCase("z", [6, 7, 13, 14]));
    }

    private static object[] CreateFirstOccurrenceTestCase(
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string searchPattern,
        int expectedByteOffset)
    {
        return [searchPattern, expectedByteOffset];
    }

    private static object[] CreateAllOccurrencesTestCase(
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string searchPattern,
        int[] expectedByteOffsets)
    {
        return [searchPattern, expectedByteOffsets];
    }

    private static ScanTextSearcher CreateScanTextSearcher(string text)
    {
        return new ScanTextSearcher(new MemoryStream(Encoding.UTF8.GetBytes(text)), Encoding.UTF8);
    }

    private static ParallelScanTextSearcher CreateParallelScanTextSearcher(string text)
    {
        var encoding = Encoding.UTF8;
        const int recoveryPointByteInterval = 32;
        var recoveryPointCharacterIndices =
            Enumerable.Sequence(0, text.Length, recoveryPointByteInterval)
                .ToArray();
        var chunkedText =
            recoveryPointCharacterIndices
                .Select(x => text[x..])
                .ToArray();
        var parallelStreams = chunkedText
            .Select((s,i) =>
                new MemoryStream(encoding.GetBytes(s))
                    .WithByteOffset<Stream>(encoding.GetByteCount(text[..recoveryPointCharacterIndices[i]]))
            );
        return new ParallelScanTextSearcher(parallelStreams, encoding);
    }

    private static IEnumerable<object[]> WithAllTextSearcherVariants(string inputText, object[] testCaseData)
    {
        yield return [CreateScanTextSearcher(inputText), ..testCaseData];
        yield return [CreateParallelScanTextSearcher(inputText), ..testCaseData];
    }

    private static IEnumerable<object[]> WithAllTextSearcherVariants(string inputText, params object[][] testCaseData)
    {
        return testCaseData.SelectMany(data => WithAllTextSearcherVariants(inputText, data));
    }
}
