using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator;

public record GameState(
    Character Hero,
    GameEventLogs GameEventLog,
    int NumberOfCombatsCompleted = 0,
    int NumberOfLimitBreaks = 0,
    bool FinalBossDefeated = false
    )
{
    public GameState IncrementCombatCounter() => this with { NumberOfCombatsCompleted = NumberOfCombatsCompleted + 1 };

    public GameState RecordEvent(ILoggableGameEvent @event) => this with { GameEventLog = GameEventLog.AddRange(@event.Log()) };

    public GameState RecordEvent(GameEventLog @event) => this with { GameEventLog = GameEventLog.Add(@event) };

    public GameState RecordEvents(params ILoggableGameEvent[] events)
    {
        return events.Aggregate(this, (current, @event) => current.RecordEvent(@event));
    }
};
