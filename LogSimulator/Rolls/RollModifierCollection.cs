using System.Collections.Immutable;

namespace LogSimulator.Rolls;

public record RollModifierCollection(ImmutableHashSet<RollModifier> Set)
{
    public static RollModifierCollection Empty { get; } = new([]);
    public RollModifierCollection Add(RollModifier modifier) => CheckedAdd(modifier);

    public bool CanAdd(RollModifier modifier) => !Set.Any(m => m.Name.Equals(modifier.Name, StringComparison.InvariantCultureIgnoreCase));

    private RollModifierCollection CheckedAdd(RollModifier modifier)
    {
        if (CanAdd(modifier))
        {
            return UncheckedAdd(modifier);
        }

        throw new ArgumentException($"The modifier '{modifier.Name}' has already been added!");
    }

    private RollModifierCollection UncheckedAdd(RollModifier modifier)
    {
        return new RollModifierCollection(Set.Add(modifier));
    }

    public int Total => Set.Sum(modifier => modifier.Value);
};
