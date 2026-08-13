using System.Globalization;
using System.IO.Compression;
using System.Text;
using CsvHelper;
using JetBrains.Annotations;
using LogSimulator.Appliction.AccessPointWriting;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using ZLibWrapper;

namespace LogSimulator.Appliction;

public class Program
{
    public static readonly DirectoryInfo TempDirectory = new("C:\\Temp");

    private static readonly CompressionFilePathFactory FilePathFactory = new(
        TempDirectory,
        DateTimeOffset.Now);

    private static FileStream WriteFileStream(FileInfo fileInfo)
    {
        fileInfo.Directory?.Create();
        return File.Open(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
    }

    private static void SetUpRecoveryPointWriting(GZipWritingStreamWithRecoveryPoints writingStreamWithRecoveryPoints)
    {
        var accessPointFileStream = Create(FilePathFactory.RecoveryPointFile);
        var csvWriter = new CsvWriter(new StreamWriter(accessPointFileStream), CultureInfo.InvariantCulture);
        csvWriter.Context.RegisterClassMap<RecoveryPointOffsetCsvMap>();
        writingStreamWithRecoveryPoints.RecoveryPointWritten += csvWriter.WriteRecord;
        writingStreamWithRecoveryPoints.StreamClosed += csvWriter.Dispose;
    }

    private static FileStream Create(FileInfo fileInfo)
    {
        fileInfo.Directory?.Create();
        return File.Open(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
    }

    [MustDisposeResource]
    private static Stream GetOutputStream()
    {
        var controlGZipStream = new GZipStream(Create(FilePathFactory.BCLCompressionFile), CompressionMode.Compress);
        var zlibGZipStream = new GZipWritingStreamWithRecoveryPoints(Create(FilePathFactory.ZLibCompressionFile));
        const long megabyte = 1024 * 1024;
        var zlibGZipStreamWithRecoveryPoints = new GZipWritingStreamWithRecoveryPoints(
            Create(FilePathFactory.ZLibCompressionWithRecoveryPointsFile),
            false,
            10 * megabyte);
        SetUpRecoveryPointWriting(zlibGZipStreamWithRecoveryPoints);
        var broadcastStream = new BroadcastStream(
        [
            new SubscribedStream(controlGZipStream, false),
            new SubscribedStream(zlibGZipStream, false),
            new SubscribedStream(zlibGZipStreamWithRecoveryPoints, false),
        ]);
        return broadcastStream;
    }

    static void Main(string[] args)
    {
        const int targetMbOfLogsToWrite = 128;

        Console.WriteLine(
            "Writing logs to '{0}' until {1:N0} MB of logs have been compressed!",
            FilePathFactory.TimestampedOutputDirectory.FullName,
            targetMbOfLogsToWrite);

        const long targetNumberOfBytesToWrite = targetMbOfLogsToWrite * 1024L * 1024L;

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
        using var outputStream = GetOutputStream();
        Console.WriteLine(progressLogger.TallyLegend);
        sim.SimulateLogs(outputStream, Encoding.Default, targetNumberOfBytesToWrite);
    }

    private static bool IsRareGameState(GameState gameState)
    {
        return gameState is {NumberOfLimitBreaks: > 1};
    }
}
