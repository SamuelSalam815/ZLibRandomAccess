using LogSimulator.Rolls;

namespace LogSimulator;

public record OverworldPhase : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var resolution = CheckedRollBuilder
            .Test("Will the Hero survive the Overworld Phase?")
            .Create()
            .ResolveWith(dieRollGenerator);
        var description = resolution.GetDescription();

        return resolution.IsSuccess()
            ? new GameProgress(this, [description])
            : new GameProgress(new GameOverPhase(), [description]);
    }
}
