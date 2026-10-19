using Benchmarking.CompressionRatio.BenchmarkRunners;
using Benchmarking.CompressionRatio.Exporters;
using Benchmarking.CompressionRatio.Values;
using Benchmarking.Helpers;

namespace Benchmarking.CompressionRatio;

public class Program
{
    public static void Main(string[] args)
    {
        var outputDirectory = ImagesDirectory.Resolve(args);
        var csvPath = Path.Combine(outputDirectory, "compression-ratio-results.csv");
        var graphPath = Path.Combine(outputDirectory, "compression-ratio-graph.svg");

        const long mb = 1024L * 1024;
        var requests = CompressionRequest.AllCombinationsOf(
                [
                    CompressionMethod.GZipUsingZLib,
                    CompressionMethod.GZipUsingZLibWithFullFlushing,
                    CompressionMethod.GZipUsingBaseClassLibrary
                ],
                [
                    NormalizedCompressionLevel.Default,
                    NormalizedCompressionLevel.Fastest,
                    NormalizedCompressionLevel.Smallest
                ],
                [
                    mb * 1,
                    mb * 4,
                    mb * 8,
                    mb * 16
                ]
            )
            .OrderBy(request => request.NumberOfBytesToCompress)
            .ToList();

        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            Directory.CreateDirectory(outputDirectory);
            var runner = new CompressionBenchmarkRunner();
            var results = requests
                .Select(request => runner.Run(request, cancellationTokenSource.Token))
                .ToList();

            // Exporters only run once every request has finished, so a cancelled run leaves the old files untouched.
            using var csvStream = File.Create(csvPath);
            List<ICompressionResultExporter> exporters =
            [
                new CompressionResultCsvExporter(csvStream),
                new CompressionResultGraphExporter(graphPath),
            ];

            foreach (var exporter in exporters)
            {
                foreach (var result in results)
                {
                    exporter.Accept(result);
                }

                exporter.Complete();
                exporter.Dispose();
            }

            Console.WriteLine("Wrote {0}", graphPath);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled operation! Existing graph was left unchanged.");
        }
    }
}
