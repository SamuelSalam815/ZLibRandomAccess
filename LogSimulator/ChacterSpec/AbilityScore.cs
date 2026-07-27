using LogSimulator.Rolls;

namespace LogSimulator.ChacterSpec;

public record struct AbilityScore(AbilityType Type, int Value)
{
    public Modifier Modifier => new(Type.ToString(), Value);
};
