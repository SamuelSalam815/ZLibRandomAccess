using Benchmarking.CompressionRatio.BenchmarkRunners;
using Benchmarking.CompressionRatio.Exporters;
using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio;

public class Program
{
    private const string CsvOutputFilePath = "result.csv";
    private const string GraphOutputFilePath = "result.svg";

    public static async Task Main()
    {
        const long gb = 1024L * 1024 * 1024;
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

        var cancellationTokenSource = new CancellationTokenSource();
        var taskCompletionSource = new TaskCompletionSource();

        if (!ConfirmOverwrite())
        {
            return;
        }

        var userTerminationTask = ListenForTerminationByUserInputAsync(taskCompletionSource, cancellationTokenSource);
        var benchmarkingTask = Task.Run(
            () => RunBenchmark(requests, taskCompletionSource, cancellationTokenSource.Token),
            cancellationTokenSource.Token);

        await Task.WhenAll(
            benchmarkingTask,
            userTerminationTask);
    }

    private static bool ConfirmOverwrite()
    {
        if (!File.Exists(CsvOutputFilePath) && !File.Exists(GraphOutputFilePath))
        {
            return true;
        }

        Console.WriteLine("Continuing will overwrite files from a previous run! Enter Y/Yes to continue anyway...");
        var input = Console.ReadLine();
        if (
            !"Y".Equals(input, StringComparison.InvariantCultureIgnoreCase) &&
            !"Yes".Equals(input, StringComparison.InvariantCultureIgnoreCase))
        {
            Console.WriteLine("Compression ratio benchmarks will not be run! Exiting...");
            return false;
        }

        return true;
    }

    private static void RunBenchmark(
        List<CompressionRequest> requests,
        TaskCompletionSource taskCompletionSource,
        CancellationToken cancellationToken
    )
    {
        try
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));
            using var csvStream = File.Open(
                CsvOutputFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read);
            using var graphStream = File.Open(
                GraphOutputFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read);
            var runner = new CompressionBenchmarkRunner();
            List<ICompressionResultExporter> exporters =
            [
                new CompressionResultCsvExporter(csvStream),
                new CompressionResultGraphExporter(graphStream),
            ];

            foreach (var request in requests)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = runner.Run(request, cancellationToken);
                foreach (var exporter in exporters)
                {
                    exporter.Accept(result);
                }
            }

            foreach (var exporter in exporters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                exporter.Complete();
                exporter.Dispose();
            }

            taskCompletionSource.SetResult();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled operation!");
        }
    }

    private static Task ListenForTerminationByUserInputAsync(
        TaskCompletionSource taskCompletionSource,
        CancellationTokenSource userTerminationCancellationTokenSource)
    {
        var stopReadingInputOnTaskComplete = new CancellationTokenSource();
        taskCompletionSource.Task.ContinueWith(
            (task, _) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    stopReadingInputOnTaskComplete.Cancel();
                }
            },
            null,
            userTerminationCancellationTokenSource.Token);

        Console.WriteLine("Enter 'exit' to terminate the benchmark early...");
        return Task.Run(
            async () =>
            {
                while (true)
                {
                    var readTask = ConsoleReadAsync();
                    await Task.WhenAny(taskCompletionSource.Task, readTask);

                    if (taskCompletionSource.Task.IsCompleted)
                    {
                        return;
                    }

                    if (!"exit".Equals(await readTask, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("Enter 'exit' to terminate the process");
                        continue;
                    }

                    Console.WriteLine("Terminating benchmarks...");
                    await userTerminationCancellationTokenSource.CancelAsync();
                    return;
                }
            },
            stopReadingInputOnTaskComplete.Token);
    }

    // Note this is a bad pattern to repeat elsewhere, but the Console api forces our hand here. Worth further reading?
    private static Task<string?> ConsoleReadAsync() => Task.Run(Console.ReadLine);
}
