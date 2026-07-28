using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;

namespace LogSimulator.Logging;

public record GameEventLogTree(GameEventLog Log, ImmutableList<GameEventLogTree> ChildEvents) : GameEventLogger
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
        if (Log.IsSummary)
        {
            throw new ArgumentException("Cannot imply the structure of logs added to a summary log!");
        }

        if (newLogTree.Log.EventScope <= Log.EventScope)
        {
            throw new ArgumentException(
                "Attempting to add a log that would be a sibling or parent to the current log! " +
                "The given log cannot be added at this level because this operation would require access to the existing parent log");
        }

        if (newLogTree.Log.IsSummary && !newLogTree.ChildEvents.IsEmpty)
        {
            throw new ArgumentException(
                "Attempting to add a summary log that already has child logs. This operation is forbidden");
        }

        if (ChildEvents.IsEmpty)
        {
            return this with { ChildEvents = ChildEvents.Add(newLogTree) };
        }

        var lastChildIndex = ChildEvents.Count - 1;
        var lastChild = ChildEvents[lastChildIndex];

        if (newLogTree.Log.IsSummary)
        {
            return AddNewSummaryLog(newLogTree, lastChild, lastChildIndex);
        }

        if (newLogTree.Log.EventScope <= lastChild.Log.EventScope)
        {
            return this with {ChildEvents = ChildEvents.Add(newLogTree)};
        }

        return this with { ChildEvents = ChildEvents.SetItem(lastChildIndex, lastChild.Add(newLogTree)) };
    }

    private GameEventLogTree AddNewSummaryLog(GameEventLogTree newLogTree, GameEventLogTree lastChild, int lastChildIndex)
    {
        if (lastChild.Log.EventScope != newLogTree.Log.EventScope)
        {
            throw new ArgumentException(
                "Cannot add a summary log when the last sibling log is not at the same scope!");
        }

        if (lastChild.Log.IsSummary)
        {
            throw new ArgumentException(
                "Cannot add a summary log when the last sibling log is already a summary!");
        }

        return this with
        {
            ChildEvents = ChildEvents.SetItem(lastChildIndex, lastChild with {Log = newLogTree.Log})
        };
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
