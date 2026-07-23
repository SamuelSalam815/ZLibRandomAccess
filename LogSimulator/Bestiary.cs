using LogSimulator.Rolls;

namespace LogSimulator;

public record Bestiary(
    Rotation<Adversary> WeakAdversaries,
    Rotation<Adversary> ModerateAdversaries,
    Rotation<Adversary> StrongAdversaries,
    Rotation<Adversary> LegendaryAdversaries)
{
    public static Bestiary DefaultBestiary()
    {
        return new Bestiary(
            [
                new Adversary("Uranium Rat", new CharacterStatistics(2, 2, 1)),
                new Adversary("Possessed Hand", new CharacterStatistics(1, 3, 1)),
                new Adversary("Enraged Bee", new CharacterStatistics(1, 1, 3))
            ],
            [
                new Adversary("Young Thug", new CharacterStatistics(2, 2, 4)),
                new Adversary("Lesser Ooze", new CharacterStatistics(1, 4, 3)),
                new Adversary("Obsessive Grass Picker", new CharacterStatistics(5, 2, 1)),
            ],
            [
                new Adversary("Greater Ooze", new CharacterStatistics(5, 4, 3)),
                new Adversary("Mimic", new CharacterStatistics(6, 6, 2)),
                new Adversary("Man Eater", new CharacterStatistics(4, 2, 6)),
            ],
            [
                new Adversary("Young Dragon", new CharacterStatistics(6, 4, 8)),
                new Adversary("Yo Momma", new CharacterStatistics(1, 10, 7)),
            ]
        );
    }

    public RollRequest RollForAdversarySelection() => RollRequest.Roll(3);

    public Bestiary SelectAdversaryWith(RollResult rollResult, out Adversary adversary)
    {
        return rollResult.ResolvedValue switch
        {
            <= 10 => this with { WeakAdversaries = WeakAdversaries.Cycle(out adversary) },
            <= 13 => this with { ModerateAdversaries = ModerateAdversaries.Cycle(out adversary) },
            <= 16 => this with { StrongAdversaries = StrongAdversaries.Cycle(out adversary) },
            _ => this with { LegendaryAdversaries = LegendaryAdversaries.Cycle(out adversary) }
        };
    }
};
