using System.Collections.Immutable;
using System.Text;

namespace LogSimulator.Logging;

public record struct GameEventDescription(string Description, ImmutableList<GameEventDescription> SubDescriptions)
{
    public static implicit operator GameEventDescription(string description)
    {
        return new GameEventDescription(description, []);
    }

    public override string ToString()
    {
        return ToString(new StringBuilder(), 0).ToString();
    }

    private StringBuilder ToString(StringBuilder builder, int indentationLevel)
    {
        for (var i = 0; i < indentationLevel; i++)
        {
            builder.Append("|   ");
        }

        builder.AppendLine(Description);

        foreach (var subDescription in SubDescriptions)
        {
            subDescription.ToString(builder, indentationLevel + 1);
        }

        return builder;
    }
};
