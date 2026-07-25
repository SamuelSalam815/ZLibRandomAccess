using System.Diagnostics.CodeAnalysis;

namespace LogSimulator.Logging;

public record GameEventLogger(List<string> EventDetails)
{
    public GameEventLogger() : this([])
    {
    }

    public GameEventLogger Log(string eventInfo)
    {
        EventDetails.Add(eventInfo + '\n');
        return this;
    }

    public GameEventLogger Log(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string unformattedEventInfo,
        params object[] substitutions)
    {
        return Log(string.Format(unformattedEventInfo, substitutions));
    }
};
