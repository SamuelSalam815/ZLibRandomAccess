using LogSimulator.Rolls;

namespace LogSimulator;

public record OverworldPhase : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var resolution = RollBuilder
            .StandardRoll()
            .WithAdvantage()
            .AgainstStandardDifficulty("Will the Hero survive the Overworld Phase?")
            .ResolveWith(dieRollGenerator);

        return resolution.IsSuccess()
            ? new GameProgress(this, [resolution])
            : new GameProgress(new GameOverPhase(), [resolution]);
    }
}
