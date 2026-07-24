using LogSimulator.Rolls;

namespace LogSimulator;

public record GameOverPhase : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator) => new(null, []);
}
