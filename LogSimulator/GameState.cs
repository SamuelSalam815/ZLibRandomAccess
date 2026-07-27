using System.Collections.Immutable;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator;

// TODO: maintain a record of all describable game events
public record GameState(
    Character Hero,
    ImmutableList<IDescribableGameEvent> GameEvents,
    int NumberOfCombatsCompleted = 0,
    int NumberOfLimitBreaks = 0,
    bool FinalBossDefeated = false
    )
{
    public GameState(Character hero) : this(hero, [])
    {
    }

    public GameState IncrementCombatCounter() => this with { NumberOfCombatsCompleted = NumberOfCombatsCompleted + 1 };

    public GameState RecordEvent(IDescribableGameEvent @event) => this with { GameEvents = GameEvents.Add(@event) };
    public GameState RecordEvents(params IDescribableGameEvent[] events)
    {
        return events.Aggregate(this, (current, @event) => current.RecordEvent(@event));
    }
};
