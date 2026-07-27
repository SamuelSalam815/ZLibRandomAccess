using System.Collections.Immutable;
using LogSimulator.Logging;

namespace LogSimulator;

public record GameOverPhase : GamePhase
{
    private GameOverPhase(GameState GameState) : base(GameState)
    {
    }

    private class GameOverEvent(GameState gameState) : IDescribableGameEvent
    {
        public GameEventDescription DescribeEvent()
        {
            var message = gameState.FinalBossDefeated
                ? $"{gameState.Hero.Name} beat the final boss after {gameState.NumberOfCombatsCompleted} encounters and {gameState.NumberOfLimitBreaks} limit breaks!"
                : $"{gameState.Hero.Name} was defeated after {gameState.NumberOfCombatsCompleted} encounters and {gameState.NumberOfLimitBreaks} limit breaks!";

            return new GameEventDescription(
                message,
                gameState.GameEvents.Select(e => e.DescribeEvent()).ToImmutableList());
        }
    }

    public static GameOverPhase CreateFrom(GameState gameState)
    {
        var gameOverEvent =  new GameOverEvent(gameState);
        return new GameOverPhase(gameState with {GameEvents = [gameOverEvent]});
    }

    public bool DidWin => GameState.FinalBossDefeated;

    public override GamePhase? ProgressGame(DieRollGenerator dieRollGenerator) => null;

}
