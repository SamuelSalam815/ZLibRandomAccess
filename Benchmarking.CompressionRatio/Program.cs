using Benchmarking.CompressionRatio.BenchmarkRunners;
using Benchmarking.CompressionRatio.Exporters;
using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio;

public class Program
{
    private const string CsvOutptutFilePath = "result.csv";

    private const string GraphOutputFilePath = "result.png";

    public static async Task Main()
    {
        const long gb = 1024 * 1024 * 1024;
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
                    gb * 1,
                    gb * 4,
                    gb * 8,
                    gb * 16
                ]
            )
            .OrderBy(request => request.NumberOfBytesToCompress)
            .ToList();

        var cancellationTokenSource = new CancellationTokenSource();
        var taskCompletionSource = new TaskCompletionSource();

        var userTerminationTask = SetUpTerminationByUserInput(taskCompletionSource, cancellationTokenSource);
        var benchmarkingTask = SetUpBenchmarkTask(requests, taskCompletionSource, cancellationTokenSource.Token);

        await Task.WhenAll(
            benchmarkingTask,
            userTerminationTask);
    }

    private static Task SetUpBenchmarkTask(
        List<CompressionRequest> requests,
        TaskCompletionSource taskCompletionSource,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
            {
                try
                {
                    // await using var csvStream = File.Open(CsvOutputFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                    var runner = new CompressionBenchmarkRunner();
                    List<ICompressionResultExporter> exporters =
                    [
                        new FakeCompressionResultExporter("Csv Exporter"),
                        new FakeCompressionResultExporter("Graph Exporter"),
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
                    }

                    taskCompletionSource.SetResult();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Cancelled operation!");
                }
            },
            cancellationToken);
    }

    private static Task SetUpTerminationByUserInput(
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

        Console.WriteLine("Enter 'exit' to terminate the process");
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
