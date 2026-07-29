using System.IO.Compression;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator.Appliction;

class Program
{
    private static readonly DirectoryInfo OutputDirectory = new DirectoryInfo("C:\\Temp");

    private record FileOutputs(FileInfo ControlOutput, FileInfo OutputWithRecoveryPoints);

    private static FileOutputs GetOutputFilePaths()
    {
        return new FileOutputs(
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    $"{DateTime.Now:yyyy-MM-ddTHH.mm.ss} SimulatedLogs.log.gz")),
            new FileInfo(
                Path.Join(
                    OutputDirectory.FullName,
                    $"{DateTime.Now:yyyy-MM-ddTHH.mm.ss} SimulatedLogs with Recovery Points.log.gz")));
    }

    [MustDisposeResource]
    private static StreamWriter GetOutputStream(FileOutputs outputs)
    {
        outputs.ControlOutput.Directory?.Create();
        var controlFile = File.Open(outputs.ControlOutput.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        // TODO: construct stream that write recovery points at a fixed byte offset interval to the recovery point file path
        var compressionStream = new GZipStream(controlFile, CompressionMode.Compress);
        return new StreamWriter(compressionStream);
    }

    static void Main(string[] args)
    {
        var outputFiles = GetOutputFilePaths();
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
