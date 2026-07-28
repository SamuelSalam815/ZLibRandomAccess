using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;

namespace LogSimulator.Logging;

public record GameEventLogTree(GameEventLog Log, ImmutableList<GameEventLogTree> ChildEvents, bool AcceptingChildren = true) : GameEventLogger
{
    public virtual bool Equals(GameEventLogTree? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return base.Equals(other) && Log.Equals(other.Log) &&
               ChildEvents
                   .Zip(other.ChildEvents, (a, b) => a.Equals(b))
                   .All(b => b);
    }

    public override int GetHashCode()
    {
        return ChildEvents.Aggregate(
            Log.GetHashCode(),
            (hashCode, childEvent) => HashCode.Combine(hashCode, childEvent.GetHashCode()));
    }

    [MustUseReturnValue]
    public override GameEventLogTree Add(GameEventLogTree newLogTree)
    {
        if (!AcceptingChildren)
        {
            throw new InvalidOperationException("Cannot add a log to a log tree that is not accepting child logs!");
        }

        if (newLogTree.Log.EventScope <= Log.EventScope)
        {
            throw new ArgumentException(
                "Attempting to add a log that would be a sibling or parent to the current log! " +
                "The given log cannot be added at this level because this operation would require access to the existing parent log");
        }

        if (ChildEvents.IsEmpty)
        {
            return this with { ChildEvents = ChildEvents.Add(newLogTree) };
        }

        var lastChildIndex = ChildEvents.Count - 1;
        var lastChild = ChildEvents[lastChildIndex];

        if (!lastChild.AcceptingChildren || newLogTree.Log.EventScope <= lastChild.Log.EventScope)
        {
            return this with { ChildEvents = ChildEvents.Add(newLogTree) };
        }

        return this with { ChildEvents = ChildEvents.SetItem(lastChildIndex, lastChild.Add(newLogTree)) };
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
