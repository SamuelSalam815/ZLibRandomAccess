using System.Collections.Immutable;

namespace LogSimulator.Rolls;

public record ModifierCollection(ImmutableHashSet<Modifier> Set)
{
    public static ModifierCollection Empty { get; } = new([]);
    public ModifierCollection Add(Modifier modifier) => CheckedAdd(modifier);

    public bool CanAdd(Modifier modifier) => !Set.Any(m => m.Name.Equals(modifier.Name, StringComparison.InvariantCultureIgnoreCase));

    private ModifierCollection CheckedAdd(Modifier modifier)
    {
        if (CanAdd(modifier))
        {
            return UncheckedAdd(modifier);
        }

        throw new ArgumentException($"The modifier '{modifier.Name}' has already been added!");
    }

    private ModifierCollection UncheckedAdd(Modifier modifier)
    {
        return new ModifierCollection(Set.Add(modifier));
    }

    public int Total => Set.Sum(modifier => modifier.Value);

    public bool IsEmpty => Set.IsEmpty;
};
