namespace LogSimulator.Rolls;

public record struct Modifier(string Name, int Value)
{
    public static readonly Modifier NoModifier = default;
};
