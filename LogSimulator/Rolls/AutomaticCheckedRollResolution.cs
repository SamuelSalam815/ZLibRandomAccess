using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record AutomaticCheckedRollResolution(CheckedRollRequest Request, bool IsSuccess) : IDescribableGameEvent
{
    public GameEventDescription DescribeEvent()
    {
        return $"Automatically resolved '{Request.Question}' to {(IsSuccess ? "YES" : "NO")}";
    }
}
