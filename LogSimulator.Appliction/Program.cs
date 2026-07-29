using System.IO.Compression;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using ZLibWrapper;

namespace LogSimulator.Appliction;

class Program
{
    private static readonly DirectoryInfo OutputDirectory = new("C:\\Temp");

    private record FileOutputs(
        FileInfo ControlOutput,
        FileInfo OutputUsingZlib,
        FileInfo OutputUsingZlibWithRecoveryPoints);

    private static FileOutputs GetOutputFilePaths(DateTime outputTimestamp)
    {
        var timestampedDirectoryName = $"{outputTimestamp:yyyy-MM-ddTHH.mm.ss}";
        return new FileOutputs(
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    timestampedDirectoryName ,
                    $"{timestampedDirectoryName} SimulatedLogs BCL Compression.log.gz")),
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    timestampedDirectoryName ,
                    $"{timestampedDirectoryName} SimulatedLogs zlib Compression.log.gz")),
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    timestampedDirectoryName ,
                    $"{timestampedDirectoryName} SimulatedLogs zlib Compression with Recovery Points.log.gz")));
    }

    private static FileStream OpenFileStream(FileInfo fileInfo)
    {
        fileInfo.Directory?.Create();
        return File.Open(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
    }

    [MustDisposeResource]
    private static StreamWriter GetOutputStream(FileOutputs outputs)
    {
        var controlGZipStream = new GZipStream(OpenFileStream(outputs.ControlOutput), CompressionMode.Compress);
        var zlibGZipStream = new GZipRecoveryPointStream(OpenFileStream(outputs.OutputUsingZlib), false, null);
        const long megabyte = 1024 * 1024;
        var zlibGZipStreamWithRecoverPoints = new GZipRecoveryPointStream(
            OpenFileStream(outputs.OutputUsingZlibWithRecoveryPoints),
            false,
            10 * megabyte);
        var broadcastStream = new BroadcastStream(
        [
            new SubscribedStream(controlGZipStream, false),
            new SubscribedStream(zlibGZipStream, false),
            new SubscribedStream(zlibGZipStreamWithRecoverPoints, false),
        ]);
        return new StreamWriter(broadcastStream);
    }

    static void Main(string[] args)
    {
        var outputFiles = GetOutputFilePaths(DateTime.Now);
        const int targetGameSimCount = 60_000;

        Console.WriteLine(
            "Writing logs to '{0}' until {1:N0} games have been simulated!",
            outputFiles,
            targetGameSimCount);

        using var outputStream = GetOutputStream(outputFiles);
        var random = new Random((int)DateTime.UtcNow.Ticks);
        var initialHero = new Character("Hero X", new StatBlock(6, 5, 5));
        var progressLogger = new GameSimTally(50, 60);
        for (int gameIndex = 0; gameIndex < targetGameSimCount; gameIndex++)
        {
            var startingTime = new DateTimeOffset(2025, 05, 5, 12, 48, 30, TimeSpan.Zero);
            var startingGameState = new GameState(initialHero, new GameEventLogs(startingTime));
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

            finalGameState = finalGameState.RecordEvent(GameEventLog.GlobalEvent($"Completed game sim index {gameIndex}"));

            outputStream.Write(finalGameState.GameEventLog);
            progressLogger.MarkGameSimulated();

            if (IsRareGameState(currentGamePhase.GameState))
            {
                progressLogger.MakeNextMarkSpecial();
            }
        }
    }

    private static bool IsRareGameState(GameState gameState)
    {
        return gameState is {NumberOfLimitBreaks: > 1, FinalBossDefeated: false};
    }
}
