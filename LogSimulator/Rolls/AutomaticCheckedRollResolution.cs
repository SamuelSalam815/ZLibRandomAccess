namespace LogSimulator.Rolls;

public sealed record AutomaticCheckedRollResolution(CheckedRollRequest Request, bool DidSucceed) : IRollResolution
{
    public bool IsSuccess() => DidSucceed;

    public GameEventDescription GetDescription()
    {
        var result = new GameEventDescription();
        result.AddLine("Testing '{0}'...", Request.Question);
        result.AddLine(DidSucceed ? "Automatically passed test!" : "Automatically failed test!");
        return result;
    }
}
