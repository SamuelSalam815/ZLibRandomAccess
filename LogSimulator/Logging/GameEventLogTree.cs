using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;

namespace LogSimulator.Logging;

public record GameEventLogTree(GameEventLog Log, ImmutableList<GameEventLogTree> ChildEvents) : GameEventLogger
{

    [MustUseReturnValue]
    public override GameEventLogTree Add(GameEventLogTree log)
    {
        throw new NotImplementedException();
    }

    public override GameEventLogTree AsTree() => this;

    private StringBuilder BuildString(StringBuilder stringBuilder, int indentationLevel)
    {
        stringBuilder = Enumerable.Repeat("|   ", indentationLevel)
            .Aggregate(stringBuilder, (builder, indent) => builder.Append(indent));
        stringBuilder.AppendLine(Log.ToString());
        return ChildEvents.Aggregate(stringBuilder, (builder, logTree) => logTree.BuildString(builder, indentationLevel + 1));
    }

    public override string ToString()
    {
        return BuildString(new StringBuilder(), 0).ToString();
    }
};
