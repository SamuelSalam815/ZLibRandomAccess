using System.IO;
using LogSimulator;
using ZLibWrapper;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressor
{
    public async Task Run(
        LogFileCompressionJob job,
        IProgress<long> numberOfBytesWrittenReporter,
        IProgress<string> gameSimulationLogReporter,
        CancellationToken cancellationToken
    )
    {
        await using var outputFileStream = File.Open(job.OutputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

        await using var recoveryPointFileStream = job.RecoveryPointFilePath is null
            ? Stream.Null
            : File.Open(job.RecoveryPointFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

        await using var compressor = new GZipWritingStreamWithRecoveryPoints(outputFileStream, recoveryPointByteInterval: job.RecoveryPointByteInterval);
        await using var accessPointStream = new RecoveryPointOffsetCsvWriter(recoveryPointFileStream, job.Encoding);
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
            () => logSimulator.SimulateLogs(compressor, job.Encoding, job.RequestedUncompressedLogSizeInBytes),
            cancellationToken);

        var gameSimReportingTask = Task.Run(async () =>
        {
            while (!simulationTask.IsCompleted)
            {
                await Task.Delay(2000, cancellationToken);
                if (lastGameSimulated is null)
                {
                    continue;
                }

                var logs = lastGameSimulated.GameEventLog.Logs;
                var randomIndex = Random.Shared.Next(0, logs.Count);
                var log = logs[randomIndex].Payload.Description;
                gameSimulationLogReporter.Report(log);
            }
        },
        cancellationToken);

        var progressReportingTask = Task.Run(async () =>
        {
            while (!simulationTask.IsCompleted)
            {
                await Task.Delay(250, cancellationToken);
                numberOfBytesWrittenReporter.Report(compressor.TotalBytesWritten);
            }
        },
        cancellationToken);


        await Task.WhenAll(
            progressReportingTask,
            gameSimReportingTask,
            simulationTask);
    }
}
