using Benchmarking.CompressionRatio.Values;
using Benchmarking.Helpers;

namespace Benchmarking.CompressionRatio.Exporters;

public sealed class CompressionResultGraphExporter(string outputFilePath) : ICompressionResultExporter
{
    private static readonly string[] MethodOrder =
    [
        MethodName(CompressionMethod.GZipUsingBaseClassLibrary),
        MethodName(CompressionMethod.GZipUsingZLib),
        MethodName(CompressionMethod.GZipUsingZLibWithFullFlushing),
    ];

    private static readonly string[] LevelOrder =
    [
        LevelName(NormalizedCompressionLevel.Fastest),
        LevelName(NormalizedCompressionLevel.Default),
        LevelName(NormalizedCompressionLevel.Smallest),
    ];

    private readonly List<CompressionResult> _results = [];

    public void Accept(CompressionResult result)
    {
        _results.Add(result);
    }

    public void Complete()
    {
        PlotModelSvgWriter.Write(BuildChart(_results), outputFilePath);
    }

    public void Dispose()
    {
        // No resources to release
    }

    /// <summary>
    /// One bar per method and level: the space saved, averaged over every uncompressed size that was measured.
    /// </summary>
    public static List<BarChartBar<string, string>> ToBars(IEnumerable<CompressionResult> results)
    {
        return results
            .GroupBy(result => (result.Request.CompressionMethod, result.Request.CompressionLevel))
            .Select(group => new BarChartBar<string, string>(
                MethodName(group.Key.CompressionMethod),
                LevelName(group.Key.CompressionLevel),
                group.Average(result => 100d * (1d - (double)result.NumberOfBytesAfterCompression /
                    result.Request.NumberOfBytesToCompress))))
            .ToList();
    }

    private static OxyPlot.PlotModel BuildChart(List<CompressionResult> results)
    {
        var sizes = results.Select(result => result.Request.NumberOfBytesToCompress).Distinct().Order().ToList();
        var sizeRange = sizes.Count == 0
            ? string.Empty
            : $" Averaged over {sizes.First() / (1024 * 1024)}-{sizes.Last() / (1024 * 1024)} MB of simulated logs.";

        return new BarChartBuilder<string, string>(
            new BarChartConfiguration<string, string>
            {
                Title = "Compression ratio: cost of full flushes",
                CategoryAxisLabel = "Gzip implementation",
                ValueAxisLabel = "Space saved (%, higher is better)." + sizeRange,
                SeriesLabel = "Compression level",
                CategoryComparer = OrderedBy(MethodOrder),
                SeriesComparer = OrderedBy(LevelOrder),
                MinimumValue = 0,
                ValueLabelFormat = "{0:0.00}",
            }).Build(ToBars(results));
    }

    private static Comparer<string> OrderedBy(string[] order) =>
        Comparer<string>.Create((x, y) => Array.IndexOf(order, x).CompareTo(Array.IndexOf(order, y)));

    private static string MethodName(CompressionMethod method) => method switch
    {
        CompressionMethod.GZipUsingBaseClassLibrary => ".NET GZipStream",
        CompressionMethod.GZipUsingZLib => "zlib",
        CompressionMethod.GZipUsingZLibWithFullFlushing => "zlib + full flush every 1 MB",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
    };

    private static string LevelName(NormalizedCompressionLevel level) => level switch
    {
        NormalizedCompressionLevel.Fastest => "Fastest",
        NormalizedCompressionLevel.Default => "Default",
        NormalizedCompressionLevel.Smallest => "Smallest",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
    };
}
