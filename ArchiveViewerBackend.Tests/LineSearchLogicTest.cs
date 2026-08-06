using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.LineSearch;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(LineSearchLogic))]
public class LineSearchLogicTest
{

    [TestMethod]
    public void EmptyStringProducesNoMatches_WhenRegexMatchesNonEmptyStrings()
    {
        _ = new LineSearchLogic(new EncodedStringBuilder(string.Empty),".+").NextMatch(out var match);
        match.ShouldBeNull();
    }

    [TestMethod]
    [DataRow("Hello, World!", "e.+o", "ello, Wo", 1)]
    [DataRow("Hello, World!", ",", ",", 5)]
    public void Regex_MatchesAreExpected(
        string text,
        string regex,
        string expectedMatchText,
        int expectedMatchByteOffset
        )
    {
        _ = new LineSearchLogic(new EncodedStringBuilder(text), regex).NextMatch(out var match);
        match.ShouldNotBeNull();
        match.Value.MatchText.ShouldBe(expectedMatchText);
        match.Value.ByteOffset.ShouldBe(expectedMatchByteOffset);
    }

    [TestMethod]
    [DataRow("Hello, World!", "\\d")]
    [DataRow("Hello, World!", "load")]
    public void NoMatches_ReturnsNull(
        string text,
        string regex
    )
    {
        _ = new LineSearchLogic(new EncodedStringBuilder(text), regex).NextMatch(out var match);
        match.ShouldBeNull();
    }

    public record MultipleMatchTestCase(
        string Input,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string Regex,
        IEnumerable<LineSearchResult> ExpectedMatches);

    public static IEnumerable<object?[]> MultipleMatchTestCases()
    {
        yield return [new MultipleMatchTestCase(
            "Hello",
            @"l\w",
            [
                new LineSearchResult("ll", 2),
                new LineSearchResult("lo", 3),
            ])];
    }

    [TestMethod]
    [DynamicData(nameof(MultipleMatchTestCases))]
    [Timeout(150)]
    public void CanIterateThroughMultipleMatches(MultipleMatchTestCase testCase)
    {
        // todo string -> encoded string implicit operator to deduplicate code
        var searchLogic = new LineSearchLogic(new EncodedStringBuilder(testCase.Input), testCase.Regex);
        var actualMatches = new List<LineSearchResult>();
        for (
            searchLogic = searchLogic.NextMatch(out var match);
            match is not null;
            searchLogic = searchLogic.NextMatch(out match)
        )
        {
            actualMatches.Add(match.Value);
        }

        actualMatches.ShouldBe(testCase.ExpectedMatches);
    }

    [TestMethod]
    public void CanChangeSearchPattern()
    {
        var newPattern = "World!";
        _ = new LineSearchLogic(new EncodedStringBuilder("Hello, World!"), "Hello")
            .SetSearchPattern(new Regex(newPattern))
            .NextMatch(out var match);

        match.ShouldNotBeNull().MatchText.ShouldBe(newPattern);
    }
}
