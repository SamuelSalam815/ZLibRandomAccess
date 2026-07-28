using System.Collections.Immutable;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator;

public record GameState(
    Character Hero,
    GameEventLogTree GameEventLog,
    int NumberOfCombatsCompleted = 0,
    int NumberOfLimitBreaks = 0,
    bool FinalBossDefeated = false
    )
{
    public GameState IncrementCombatCounter() => this with { NumberOfCombatsCompleted = NumberOfCombatsCompleted + 1 };

    public GameState RecordEvent(ILoggableGameEvent @event) => this with { GameEventLog = GameEventLog.Add(@event) };
    public GameState RecordEvent(GameEventLogTree @event) => this with { GameEventLog = GameEventLog.Add(@event) };

    public GameState RecordEvents(params ILoggableGameEvent[] events)
    {
        return events.Aggregate(this, (current, @event) => current.RecordEvent(@event));
    }
};
