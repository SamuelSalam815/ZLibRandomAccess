using System.IO;
using ErrorOr;
using LogSimulator;
using ZLibWrapper;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressionJobRunner(LogFileCompressionRequest request)
{
    public async Task Run(
        IProgress<ErrorOr<LogFileCompressionProgressReport>> progress,
        CancellationToken cancellationToken = default)
    {
        Stream outputFileStream;
        Stream recoveryPointFileStream;
        try
        {
            AcquireFileHandles(out outputFileStream, out recoveryPointFileStream);
        }
        catch (Exception exception)
        {
            progress.Report(Error.Failure(description: exception.Message));
            return;
        }

        var didOperationFail = true;
        try
        {
            await DoWork(outputFileStream, recoveryPointFileStream, progress, cancellationToken);
            didOperationFail = false;
        }
        catch (TaskCanceledException)
        {
            didOperationFail = true;
            progress.Report(Error.Failure(description: "User cancelled job"));
        }
        catch (Exception exception)
        {
            didOperationFail = true;
            progress.Report(Error.Failure(description: exception.Message));
        }
        finally
        {
            await outputFileStream.DisposeAsync();
            await recoveryPointFileStream.DisposeAsync();
            if (didOperationFail)
            {
                DeleteOutputFiles();
            }
        }
    }

    private void DeleteOutputFiles()
    {
        TryDelete(request.OutputFilePath);
        if (request.RecoveryPointFileDetails is { FilePath: var recoveryPointFilePath })
        {
            TryDelete(recoveryPointFilePath);
        }
    }

    private void TryDelete(string recoveryPointFilePath)
    {
        // Intentionally suppress expected errors
        try
        {
            File.Delete(recoveryPointFilePath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private void AcquireFileHandles(out Stream outputFileStream, out Stream recoveryPointFileStream)
    {
        outputFileStream = File.Open(request.OutputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

        recoveryPointFileStream = request.RecoveryPointFileDetails is { } recoveryPointFileDetails
            ? File.Open(recoveryPointFileDetails.FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read)
            : Stream.Null;
    }

    private async Task DoWork(
        Stream outputFileStream,
        Stream recoveryPointFileStream,
        IProgress<ErrorOr<LogFileCompressionProgressReport>> progress,
        CancellationToken cancellationToken = default)
    {
        await using var compressor = new GZipWritingStreamWithRecoveryPoints(
            outputFileStream,
            recoveryPointByteInterval: request.RecoveryPointFileDetails?.RecoveryPointInterval.ByteCount);
        await using var accessPointStream = new RecoveryPointOffsetCsvWriter(recoveryPointFileStream, request.Encoding);
        compressor.RecoveryPointWritten += accessPointStream.Write;

        var logSimulator = new LogSimulator.Simulator.LogSimulator();
        GameState? lastGameSimulated = null;
        var lastGameSemaphore = new SemaphoreSlim(1);

        logSimulator.GameEnded += game =>
        {
            lastGameSemaphore.Wait(cancellationToken);
            lastGameSimulated = game;
            lastGameSemaphore.Release();
        };

        var simulationTask = Task.Run(
            () => logSimulator.SimulateLogs(compressor, request.Encoding, request.RequestedLogSize),
            cancellationToken);

        var progressReportingTask = Task.Run(
            async () =>
            {
                while (!simulationTask.IsCompleted)
                {
                    await Task.Delay(250, cancellationToken);
                    string? status = null;
                    if (lastGameSimulated is not null)
                    {
                        var logs = lastGameSimulated.GameEventLog.Logs;
                        var randomIndex = Random.Shared.Next(0, logs.Count);
                        status = logs[randomIndex].Payload.Description;
                    }

                    progress.Report(
                        new LogFileCompressionProgressReport(
                            compressor.TotalBytesWritten,
                            status
                        ));
                }
            },
            cancellationToken);

        await Task.WhenAll(
            progressReportingTask,
            simulationTask);

        progress.Report(new LogFileCompressionProgressReport(request.RequestedLogSize, "Finished writing logs!", true));
    }
}
