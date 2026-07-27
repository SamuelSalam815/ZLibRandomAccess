namespace LogSimulator;

public record GameOverPhase(GameState GameState) : GamePhase(GameState)
{
    public bool DidWin => GameState.FinalBossDefeated;
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator) => new(null, []);
}
