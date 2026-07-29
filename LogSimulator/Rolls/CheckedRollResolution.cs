using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record CheckedRollResolution(CheckedRollRequest Request, RollResolution Roll) : ILoggableGameEvent
{
    public bool IsSuccess => Roll.RolledTotal >= Request.Difficulty;

    public IEnumerable<GameEventLog> Log()
    {
        yield return GameEventLog.RollEvent($"{Request.Question} {(IsSuccess ? "YES" : "NO")}");
        yield return
            GameEventLog.RollEvent(
                $"Rolled value ({Roll.RolledTotal}) {(IsSuccess ? "beats" : "fails")} test difficulty ({Request.Difficulty})");

        foreach (var log in Roll.Log())
        {
            yield return log;
        }
    }
}
