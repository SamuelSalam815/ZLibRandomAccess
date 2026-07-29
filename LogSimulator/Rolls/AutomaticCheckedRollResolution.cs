using System.Collections;
using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record AutomaticCheckedRollResolution(CheckedRollRequest Request, bool IsSuccess) : ILoggableGameEvent
{
    public IEnumerable<GameEventLog> Log()
    {
        yield return
            GameEventLog.RollEvent($"Automatically resolved '{Request.Question}' to {(IsSuccess ? "YES" : "NO")}");
    }
}
