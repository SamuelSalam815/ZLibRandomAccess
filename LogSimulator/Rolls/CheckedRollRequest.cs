namespace LogSimulator.Rolls;

public record CheckedRollRequest(
    string Question,
    int Difficulty,
    RollRequest RollRequest)
{
    public AutomaticCheckedRollResolution AutoSucceed() => new(this, true);
    public AutomaticCheckedRollResolution AutoFail() => new(this, false);

    public CheckedRollResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        return new CheckedRollResolution(this, RollRequest.ResolveWith(dieRollGenerator));
    }

    public CheckedRollResolution ResolveWith(params int[] rolls)
    {
        return new CheckedRollResolution(this, RollRequest.ResolveWith(rolls));
    }
}
