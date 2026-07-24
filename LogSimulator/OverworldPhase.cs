using LogSimulator.Checks;

namespace LogSimulator;

public record OverworldPhase : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var resolution = AbilityCheckBuilder
            .Test("Will the Hero survive the Overworld Phase?")
            .Create()
            .ResolveWith(dieRollGenerator);
        var description = resolution.GetDescription();

        return resolution.IsSuccess()
            ? new GameProgress(this, [description])
            : new GameProgress(new GameOverPhase(), [description]);
    }
}
