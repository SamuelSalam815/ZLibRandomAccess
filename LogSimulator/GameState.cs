using LogSimulator.ChacterSpec;

namespace LogSimulator;

public record GameState(
    Character Hero,
    int NumberOfCombatsCompleted = 0,
    int NumberOfLimitBreaks = 0,
    bool FinalBossDefeated = false)
{
    public GameState IncrementCombatCounter() => this with { NumberOfCombatsCompleted = NumberOfCombatsCompleted + 1 };
};
