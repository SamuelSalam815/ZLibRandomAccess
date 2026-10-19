using System.Linq;
using Benchmarking.Helpers;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot.Series;
using Shouldly;

namespace Benchmarking.TextSearch.Tests;

[TestClass]
[TestSubject(typeof(TextSearchGraph))]
public class TextSearchGraphTest
{
    private static readonly (string, int, double)[] Measurements =
    [
        (nameof(BenchmarkingRegexInGzipStream.ZlibGzip_Parallel_FindAll), 256, 1_000_000_000d),
        (nameof(BenchmarkingRegexInGzipStream.SystemGzip_FindAll), 256, 4_000_000_000d),
        (nameof(BenchmarkingRegexInGzipStream.ZlibGzip_FindAll), 256, 4_500_000_000d),
    ];

    [TestMethod]
    public void ToBars_ConvertsNanosecondsToSecondsAndNamesMethods()
    {
        TextSearchGraph.ToBars(Measurements).ShouldBe(
        [
            new BarChartBar<int, string>(256, "zlib, parallel from recovery points", 1d),
            new BarChartBar<int, string>(256, ".NET GZipStream, one thread", 4d),
            new BarChartBar<int, string>(256, "zlib, one thread", 4.5d),
        ]);
    }

    [TestMethod]
    public void Build_ListsSeriesInTheOrderOfTheBenchmarks()
    {
        var model = TextSearchGraph.Build(TextSearchGraph.ToBars(Measurements));

        model.Series.Cast<BarSeries>().Select(series => series.Title).ShouldBe(
        [
            ".NET GZipStream, one thread",
            "zlib, one thread",
            "zlib, parallel from recovery points",
        ]);
    }
}
