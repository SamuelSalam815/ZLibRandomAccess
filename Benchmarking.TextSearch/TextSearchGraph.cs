using BenchmarkDotNet.Reports;
using Benchmarking.Helpers;
using OxyPlot;

namespace Benchmarking.TextSearch;

public static class TextSearchGraph
{
    private static readonly string[] SeriesOrder =
    [
        SeriesName(nameof(BenchmarkingRegexInGzipStream.SystemGzip_FindAll)),
        SeriesName(nameof(BenchmarkingRegexInGzipStream.ZlibGzip_FindAll)),
        SeriesName(nameof(BenchmarkingRegexInGzipStream.ZlibGzip_Parallel_FindAll)),
    ];

    /// <summary>
    /// One bar per benchmark method and log size: the mean time in seconds.
    /// </summary>
    public static List<BarChartBar<int, string>> ToBars(IEnumerable<(string Method, int LogSizeInMegabytes, double MeanInNanoseconds)> measurements)
    {
        return measurements
            .Select(m => new BarChartBar<int, string>(m.LogSizeInMegabytes, SeriesName(m.Method), m.MeanInNanoseconds / 1e9))
            .ToList();
    }

    public static PlotModel Build(Summary summary)
    {
        var measurements = summary.Reports.Select(report => (
            report.BenchmarkCase.Descriptor.WorkloadMethod.Name,
            (int)report.BenchmarkCase.Parameters[nameof(BenchmarkingRegexInGzipStream.UncompressedLogSizeInMegabytes)],
            report.ResultStatistics!.Mean));
        // BenchmarkDotNet describes the OS, CPU (including core counts) and runtime it ran on.
        var machine = string.Join("\n", summary.HostEnvironmentInfo.ToFormattedString());
        return Build(ToBars(measurements), machine);
    }

    public static PlotModel Build(List<BarChartBar<int, string>> bars, string? machineDescription = null)
    {
        return new BarChartBuilder<int, string>(
            new BarChartConfiguration<int, string>
            {
                Title = "Regex search over gzip-compressed logs",
                Subtitle = machineDescription,
                CategoryAxisLabel = "Uncompressed log size (MB)",
                ValueAxisLabel = "Time to find every match (seconds, lower is better)",
                SeriesLabel = "Search method",
                SeriesComparer = Comparer<string>.Create(
                    (x, y) => Array.IndexOf(SeriesOrder, x).CompareTo(Array.IndexOf(SeriesOrder, y))),
                MinimumValue = 0,
                ValueLabelFormat = "{0:0.0}",
            }).Build(bars);
    }

    private static string SeriesName(string benchmarkMethod) => benchmarkMethod switch
    {
        nameof(BenchmarkingRegexInGzipStream.SystemGzip_FindAll) => ".NET GZipStream, one thread",
        nameof(BenchmarkingRegexInGzipStream.ZlibGzip_FindAll) => "zlib, one thread",
        nameof(BenchmarkingRegexInGzipStream.ZlibGzip_Parallel_FindAll) => "zlib, parallel from recovery points",
        _ => throw new ArgumentOutOfRangeException(nameof(benchmarkMethod), benchmarkMethod, null),
    };
}
