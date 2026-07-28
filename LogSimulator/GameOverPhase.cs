using LogSimulator.Logging;

namespace LogSimulator;

public record GameOverPhase : GamePhase
{
    private GameOverPhase(GameState GameState) : base(GameState)
    {
    }

    public static GameOverPhase CreateFrom(GameState gameState)
    {
        var message = gameState.FinalBossDefeated
            ? $"{gameState.Hero.Name} beat the final boss after completing {gameState.NumberOfCombatsCompleted} encounters and performing {gameState.NumberOfLimitBreaks} limit breaks!"
            : $"{gameState.Hero.Name} was defeated after completing {gameState.NumberOfCombatsCompleted} encounters and performing {gameState.NumberOfLimitBreaks} limit breaks!";

        return new GameOverPhase(gameState.RecordEvent(GameEventLog.GameEvent(message)));
    }

    public bool DidWin => GameState.FinalBossDefeated;

    public override GamePhase? ProgressGame(DieRollGenerator dieRollGenerator) => null;

}
