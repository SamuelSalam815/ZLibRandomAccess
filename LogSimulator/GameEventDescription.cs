using System.Diagnostics.CodeAnalysis;

namespace LogSimulator;

public record GameEventDescription(List<string> EventDetails)
{
    public GameEventDescription() : this([])
    {
    }

    public GameEventDescription AddLine(string eventInfo)
    {
        EventDetails.Add(eventInfo + '\n');
        return this;
    }

    public GameEventDescription AddLine(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string unformattedEventInfo,
        params object[] substitutions)
    {
        return AddLine(string.Format(unformattedEventInfo, substitutions));
    }

    public void Append(GameEventDescription other)
    {
        EventDetails.AddRange(other.EventDetails);
    }
};
