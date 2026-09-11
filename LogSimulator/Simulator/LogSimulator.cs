using System.Text;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator.Simulator;

public class LogSimulator(
    string heroName,
    DateTimeOffset startingTime,
    Random random)
{

    public LogSimulator() : this(
        "Hero X",
        new DateTimeOffset(2025, 05, 5, 12, 48, 30, TimeSpan.Zero),
        new Random((int)DateTime.UtcNow.Ticks))
    {
    }

    /// <summary>
    /// Notifies listeners of the final game state of each game that is simulated while writing logs.
    /// </summary>
    public event Action<GameState>? GameEnded;

    public void SimulateLogs(Stream stream, Encoding encoding, long targetNumberOfBytesToWrite)
    {
        var initialHero = new Character(heroName, new StatBlock(6, 5, 5));
        var startingGameTime = startingTime;
        var numBytesWritten = 0L;
        var gameIndex = 0;
        while (numBytesWritten < targetNumberOfBytesToWrite)
        {
            var startingGameState = new GameState(initialHero, new GameEventLogs(startingGameTime));
            startingGameState =
                startingGameState.RecordEvent(GameEventLog.GlobalEvent($"Beginning game sim index {gameIndex}"));
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
            finalGameState =
                finalGameState.RecordEvent(GameEventLog.GlobalEvent($"Completed game sim index {gameIndex++}"));

            GameEnded?.Invoke(finalGameState);

            var logsForGame = finalGameState.GameEventLog.ToString();
            var logsAsBytes = encoding.GetBytes(logsForGame);
            var numBytesToWrite = Math.Min(logsAsBytes.Length, targetNumberOfBytesToWrite - numBytesWritten);
            stream.Write(logsAsBytes, 0, (int)numBytesToWrite);
            numBytesWritten += numBytesToWrite;
            var newLine = encoding.GetBytes(Environment.NewLine);

            if (numBytesWritten + newLine.Length <= targetNumberOfBytesToWrite)
            {
                stream.Write(newLine);
                numBytesWritten += newLine.Length;
            }
        }
    }
}
