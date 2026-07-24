using LogSimulator.Rolls;

namespace LogSimulator;

public record OverworldPhase : GamePhase
{
    public static readonly Test SurvivalTest = new("Will the Hero survive the Overworld Phase?", 10);

    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        return RollBuilder
            .StandardRoll()
            .Create()
            .ResolveWith(dieRollGenerator)
            .Against(SurvivalTest, out var testDescription)
            ? new GameProgress(this, [testDescription])
            : new GameProgress(new GameOverPhase(), [testDescription]);
    }
}

public record Test(string Question, int Difficulty)
{
    public bool AttemptWith(int modifiedRoll, out GameEventDescription description)
    {
        description = new GameEventDescription().AddLine("Resolving '{0}'", Question);
        if (modifiedRoll < Difficulty)
        {
            description
                .AddLine("Answer: No")
                .AddLine("Rolled value ({0}) fails to overcome test difficulty ({1})!", modifiedRoll, Difficulty);
            return false;
        }

        description
            .AddLine("Answer: Yes")
            .AddLine("Rolled value ({0}) beats test difficulty ({1})!", modifiedRoll, Difficulty);
        return true;
    }
}
