using System.IO;
using LogSimulator;
using ZLibWrapper;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressionJobRunner(LogFileCompressionRequest request)
{
    public async Task Run(IProgress<LogFileCompressionProgressReport> progress, CancellationToken cancellationToken = default)
    {
        await using var outputFileStream = File.Open(request.OutputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

        await using var recoveryPointFileStream = request.RecoveryPointFileDetails is { } recoveryPointFileDetails
            ? File.Open(recoveryPointFileDetails.FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read)
            : Stream.Null;

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
            () => logSimulator.SimulateLogs(compressor, request.Encoding, request.RequestedLogSize.ByteCount),
            cancellationToken);

        var progressReportingTask = Task.Run(async () =>
            {
                while (!simulationTask.IsCompleted)
                {
                    await Task.Delay(2000, cancellationToken);
                    string? status = null;
                    if (lastGameSimulated is not null)
                    {
                        var logs = lastGameSimulated.GameEventLog.Logs;
                        var randomIndex = Random.Shared.Next(0, logs.Count);
                        status = logs[randomIndex].Payload.Description;
                    }

                    progress.Report(new LogFileCompressionProgressReport(
                        compressor.TotalBytesWritten,
                        request.RequestedLogSize.ByteCount,
                        status
                    ));
                }
            },
            cancellationToken);

        await Task.WhenAll(
            progressReportingTask,
            simulationTask);
    }
}