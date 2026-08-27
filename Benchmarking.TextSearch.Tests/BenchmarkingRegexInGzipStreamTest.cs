using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Benchmarking.TextSearch.Tests;

[TestClass]
[TestSubject(typeof(BenchmarkingRegexInGzipStream))]
public class BenchmarkingRegexInGzipStreamTest
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    [DataRow("e", false)]
    [DataRow("Hero X was defeated after completing 36 encounters and performing 2 limit breaks", true)]
    [DataRow("Ben 10 was defeated after completing 36 encounters and performing 20 limit breaks", true)]
    [DataRow("Spiderman was defeated after completing 36 encounters and performing 3 limit breaks", true)]
    [DataRow("Jimmy was defeated after completing 1 encounters and performing 0 limit breaks", false)]
    [DataRow("Jimmy was defeated after completing 10 encounters and performing 1 limit breaks", false)]
    public void SearchRegex_MatchesExpectedText(string text, bool shouldMatch)
    {
        var match = BenchmarkingRegexInGzipStream.SearchPattern.Match(text);
        match.Success.ShouldBe(shouldMatch);
        TestContext?.Write("Match Text: '{0}'", match.Value);
    }
}
