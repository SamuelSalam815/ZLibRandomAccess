using BenchmarkDotNet.Running;
using Benchmarking.Helpers;

namespace Benchmarking.TextSearch;

public class Program
{
    /// <summary>
    /// Runs the text search benchmarks, then writes the graph to the images directory (first argument, default
    /// <c>images/</c> at the repository root). Run in Release; BenchmarkDotNet refuses to benchmark a Debug build.
    /// </summary>
    public static int Main(string[] args)
    {
        var outputDirectory = ImagesDirectory.Resolve(args);
        var summary = BenchmarkRunner.Run<BenchmarkingRegexInGzipStream>();

        if (summary.HasCriticalValidationErrors || summary.Reports.Any(report => report.ResultStatistics is null))
        {
            Console.Error.WriteLine("The benchmarks did not complete! Existing graph was left unchanged.");
            return 1;
        }

        var graphPath = Path.Combine(outputDirectory, "text-search-graph.svg");
        PlotModelSvgWriter.Write(TextSearchGraph.Build(summary), graphPath);
        Console.WriteLine("Wrote {0}", graphPath);
        return 0;
    }
}
