using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record CheckedRollResolution(CheckedRollRequest Request, RollResolution Roll) : ILoggableGameEvent
{
    public bool IsSuccess => Roll.RolledTotal >= Request.Difficulty;

    public GameEventLogTree Log()
    {
        return GameEventLog.RollEvent($"{Request.Question} {(IsSuccess ? "YES" : "NO")}")
        .AddDirectChild(GameEventLog.RollEvent($"Rolled value ({Roll.RolledTotal}) {(IsSuccess ? "beats" : "fails")} test difficulty ({Request.Difficulty})"))
        .AddDirectChild(Roll.Log());
    }
}
