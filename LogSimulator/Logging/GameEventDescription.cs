using System.Collections.Immutable;

namespace LogSimulator.Logging;

public record struct GameEventDescription(string Description, ImmutableList<GameEventDescription> SubDescriptions)
{
    public static implicit operator GameEventDescription(string description)
    {
        return new GameEventDescription(description, []);
    }
};
