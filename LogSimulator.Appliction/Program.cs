using System.IO.Compression;
using System.Text;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using ZLibWrapper;

namespace LogSimulator.Appliction;

public class Program
{
    public static readonly DirectoryInfo TempDirectory = new("C:\\Temp");

    private static FileStream WriteFileStream(FileInfo fileInfo)
    {
        fileInfo.Directory?.Create();
        return File.Open(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
    }

    private static void SetUpRecoveryPointWriting(
        CompressionFilePathFactory filePathFactory,
        GZipWritingStreamWithRecoveryPoints writingStreamWithRecoveryPoints)
    {
        var csvWriter = new RecoveryPointOffsetCsvWriter(Create(filePathFactory.RecoveryPointFile), Encoding.Default);
        writingStreamWithRecoveryPoints.RecoveryPointWritten += csvWriter.Write;
        writingStreamWithRecoveryPoints.StreamClosed += csvWriter.Dispose;
    }

    private static FileStream Create(FileInfo fileInfo)
    {
        fileInfo.Directory?.Create();
        return File.Open(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
    }

    [MustDisposeResource]
    private static Stream GetOutputStream(
        CompressionFilePathFactory filePathFactory,
        long recoveryPointByteInterval)
    {
        var controlGZipStream = new GZipStream(Create(filePathFactory.BCLCompressionFile), CompressionMode.Compress);
        var zlibGZipStream = new GZipWritingStreamWithRecoveryPoints(Create(filePathFactory.ZLibCompressionFile));
        var zlibGZipStreamWithRecoveryPoints = new GZipWritingStreamWithRecoveryPoints(
            Create(filePathFactory.ZLibCompressionWithRecoveryPointsFile),
            false,
            recoveryPointByteInterval);
        SetUpRecoveryPointWriting(filePathFactory, zlibGZipStreamWithRecoveryPoints);
        var broadcastStream = new BroadcastStream(
        [
            new SubscribedStream(controlGZipStream, false),
            new SubscribedStream(zlibGZipStream, false),
            new SubscribedStream(zlibGZipStreamWithRecoveryPoints, false),
        ]);
        return broadcastStream;
    }

    // Usage: [outputDirectory] [megabytesOfLogs] [recoveryPointIntervalKilobytes]
    static void Main(string[] args)
    {
        var outputDirectory = args.Length > 0 ? new DirectoryInfo(args[0]) : TempDirectory;
        var targetMbOfLogsToWrite = args.Length > 1 ? int.Parse(args[1]) : 128;
        var recoveryPointByteInterval = (args.Length > 2 ? long.Parse(args[2]) : 10 * 1024) * 1024;
        var filePathFactory = new CompressionFilePathFactory(outputDirectory, DateTimeOffset.Now);

        Console.WriteLine(
            "Writing logs to '{0}' until {1:N0} MB of logs have been compressed!",
            filePathFactory.TimestampedOutputDirectory.FullName,
            targetMbOfLogsToWrite);

        var targetNumberOfBytesToWrite = targetMbOfLogsToWrite * 1024L * 1024L;

        var sim = new Simulator.LogSimulator();
        var progressLogger = new GameSimTally(5, 50);

        sim.GameEnded += gameState =>
        {
            if (IsRareGameState(gameState))
            {
                progressLogger.MakeNextMarkSpecial();
            }

            progressLogger.MarkGameSimulated();
        };
        using var outputStream = GetOutputStream(filePathFactory, recoveryPointByteInterval);
        Console.WriteLine(progressLogger.TallyLegend);
        sim.SimulateLogs(outputStream, Encoding.Default, targetNumberOfBytesToWrite);
    }

    private static bool IsRareGameState(GameState gameState)
    {
        return gameState is {NumberOfLimitBreaks: > 1};
    }
}
