using System.Collections.Immutable;

namespace LogSimulator.ChacterSpec;

public record StatBlock(ImmutableDictionary<AbilityType, int> AbilityScores)
{
    public AbilityScore Get(AbilityType type) => new(type, AbilityScores[type]);

    public StatBlock(int fortitude, int agility, int prowess) : this(
        new Dictionary<AbilityType, int>
        {
            { AbilityType.Fortitude, fortitude },
            { AbilityType.Agility, agility },
            { AbilityType.Prowess, prowess },
        }.ToImmutableDictionary())
    {
    }
};
