using System.IO.Compression;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using ZLibWrapper;

namespace LogSimulator.Appliction;

class Program
{
    private static readonly DirectoryInfo OutputDirectory = new DirectoryInfo("C:\\Temp");

    private record FileOutputs(
        FileInfo ControlOutput,
        FileInfo OutputUsingZlib,
        FileInfo OutputUsingZlibWithRecoveryPoints);

    private static FileOutputs GetOutputFilePaths(DateTime outputTimestamp)
    {
        return new FileOutputs(
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    $"{outputTimestamp:yyyy-MM-ddTHH.mm.ss} SimulatedLogs BCL Compression.log.gz")),
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    $"{outputTimestamp:yyyy-MM-ddTHH.mm.ss} SimulatedLogs zlib Compression.log.gz")),
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    $"{outputTimestamp:yyyy-MM-ddTHH.mm.ss} SimulatedLogs zlib Compression with Recovery Points.log.gz")));
    }

    private static FileStream OpenFileStream(FileInfo fileInfo)
    {
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
        const int targetRareEventCount = 3;

        Console.WriteLine(
            "Writing logs to '{0}' until {1} 'rare' events have occurred",
            outputFiles,
            targetRareEventCount);

        using var outputStream = GetOutputStream(outputFiles);
        var random = new Random((int)DateTime.UtcNow.Ticks);
        var initialHero = new Character("Hero X", new StatBlock(6, 5, 5));
        var rareEventCount = 0;
        var progressLogger = new GameSimTally(50, 120);
        do
        {
            GamePhase currentGamePhase = OverworldPhase.NewGame(new GameState(initialHero, GameEventLog.GlobalEvent("Simulating a new game!")));
            GamePhase? nextGamePhase;
            do
            {
                nextGamePhase = currentGamePhase.ProgressGame(diceSize => random.Next(1, diceSize + 1));

                if (nextGamePhase is not null)
                {
                    currentGamePhase = nextGamePhase;
                }
            } while (nextGamePhase is not null);

            outputStream.Write(currentGamePhase.GameState.GameEventLog);
            progressLogger.MarkGameSimulated();

            if (IsRareGameState(currentGamePhase.GameState))
            {
                Console.WriteLine();
                Console.WriteLine("Simulated a rare game!");
                progressLogger.MakeNextMarkOnNewLine();
                rareEventCount++;
            }
        } while (rareEventCount < targetRareEventCount);
    }

    private static bool IsRareGameState(GameState gameState)
    {
        return gameState is {NumberOfLimitBreaks: > 1, FinalBossDefeated: false};
    }
}
