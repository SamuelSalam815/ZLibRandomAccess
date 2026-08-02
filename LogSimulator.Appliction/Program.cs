using System.Globalization;
using System.IO.Compression;
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
    private static StreamWriter GetOutputStream()
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
        return new StreamWriter(broadcastStream);
    }

    static void Main(string[] args)
    {
        const int targetGameSimCount = 120_000;

        Console.WriteLine(
            "Writing logs to '{0}' until {1:N0} games have been simulated!",
            FilePathFactory.TimestampedOutputDirectory.FullName,
            targetGameSimCount);

        using var outputStream = GetOutputStream();
        var random = new Random((int)DateTime.UtcNow.Ticks);
        var initialHero = new Character("Hero X", new StatBlock(6, 5, 5));
        var progressLogger = new GameSimTally(100, 60);
        Console.WriteLine(progressLogger.TallyLegend);
        var startingGameTime = new DateTimeOffset(2025, 05, 5, 12, 48, 30, TimeSpan.Zero);
        for (var gameIndex = 0; gameIndex < targetGameSimCount; gameIndex++)
        {
            var startingGameState = new GameState(initialHero, new GameEventLogs(startingGameTime));
            startingGameState = startingGameState.RecordEvent(GameEventLog.GlobalEvent($"Beginning game sim index {gameIndex}"));
            GamePhase currentGamePhase = OverworldPhase.NewGame(startingGameState);
            GamePhase? nextGamePhase;
            do
            {
                nextGamePhase = currentGamePhase.ProgressGame(diceSize => random.Next(1, diceSize + 1));

                if (nextGamePhase is not null)
                {
                    currentGamePhase = nextGamePhase;
                }
            } while (nextGamePhase is not null);

            var finalGameState = currentGamePhase.GameState;

            startingGameTime = finalGameState.GameEventLog.CurrentTime;
            finalGameState = finalGameState.RecordEvent(GameEventLog.GlobalEvent($"Completed game sim index {gameIndex}"));

            outputStream.WriteLine(finalGameState.GameEventLog);

            if (IsRareGameState(currentGamePhase.GameState))
            {
                progressLogger.MakeNextMarkSpecial();
            }

            progressLogger.MarkGameSimulated();
        }
    }

    private static bool IsRareGameState(GameState gameState)
    {
        return gameState is {NumberOfLimitBreaks: > 1};
    }
}
