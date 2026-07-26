namespace LogSimulator.Rolls;

public record struct RollModifier(string Name, int Value)
{
    public static readonly RollModifier NoModifier = default;
};
