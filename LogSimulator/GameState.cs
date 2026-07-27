using LogSimulator.ChacterSpec;

namespace LogSimulator;

// TODO: maintain a record of all describable game events
public record GameState(
    Character Hero,
    int NumberOfCombatsCompleted = 0,
    int NumberOfLimitBreaks = 0,
    bool FinalBossDefeated = false)
{
    public GameState IncrementCombatCounter() => this with { NumberOfCombatsCompleted = NumberOfCombatsCompleted + 1 };
};
