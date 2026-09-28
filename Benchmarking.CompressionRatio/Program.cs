using System.Threading.Tasks.Dataflow;
using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio;

public class Program
{
    //
    public static async Task Main()
    {
        const long gb = 1024 * 1024 * 1024;
        const string csvOutptutFilePath = "result.csv";
        const string graphOutputFilePath = "result.png";
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
        await using var csvStream = File.Open(csvOutptutFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

        var userTerminationTask = SetUpTerminationByUserInput(taskCompletionSource, cancellationTokenSource);
        var csvWriting = SetUpFileWritingTask(csvStream, cancellationTokenSource.Token);
        var benchmarkingTask = SetUpBenchmarkRunnerTask(requests, csvWriting, cancellationTokenSource);
        var graphGenerationTask = SetUpGraphGenerationTask(csvWriting.Completion, csvStream, cancellationTokenSource);

        await Task.WhenAll(
            benchmarkingTask,
            csvWriting.Completion,
            graphGenerationTask,
            userTerminationTask);
    }

    private static Task SetUpGraphGenerationTask(
        Task resultWritingTask,
        FileStream outputStream,
        CancellationTokenSource cancellationTokenSource)
    {
        return Task.Run(async () =>
            {
                await resultWritingTask;
                outputStream.Position = 0;
                Console.WriteLine("Graphing results...");

                // TODO: make the graphs
                throw new NotImplementedException();
            },
            cancellationTokenSource.Token);
    }

    private static Task SetUpBenchmarkRunnerTask(
        List<CompressionRequest> requests,
        ActionBlock<CompressionResult> outputWriter,
        CancellationTokenSource cancellationTokenSource)
    {
        var runnerTask = Task.Run(
            () =>
            {
                var runner = new CompressionBenchmarkRunner();
                foreach (var request in requests)
                {
                    Console.WriteLine($"Running request '{request}'...");
                    outputWriter.Post(runner.Run(request));
                }

                outputWriter.Complete();
            },
            cancellationTokenSource.Token);
        return runnerTask;
    }

    private static ActionBlock<CompressionResult> SetUpFileWritingTask(Stream outputStream, CancellationToken token)
    {
        using var resultWriter = new CompressionResultCsvWriter(outputStream);
        ActionBlock<CompressionResult> resultConsumer = new(
            resultWriter.Write,
            new ExecutionDataflowBlockOptions { CancellationToken = token });
        return resultConsumer;
    }

    private static Task SetUpTerminationByUserInput(
        TaskCompletionSource taskCompletionSource,
        CancellationTokenSource cancellationTokenSource)
    {
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

                    Console.WriteLine("Terminating process...");
                    await cancellationTokenSource.CancelAsync();
                    return;
                }
            },
            cancellationTokenSource.Token);
    }

    // Note this is a bad pattern to repeat elsewhere, but the Console api forces our hand here. Worth further reading?
    private static Task<string?> ConsoleReadAsync() => Task.Run(Console.ReadLine);
}
