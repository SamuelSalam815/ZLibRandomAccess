using JetBrains.Annotations;

namespace LogSimulator.Logging;

public abstract record GameEventLogger
{
    [MustUseReturnValue]
    public abstract GameEventLogTree Add(GameEventLogTree log);

    [MustUseReturnValue]
    public GameEventLogTree AddDirectChild(GameEventLogTree log)
    {
        var tree = AsTree();
        return tree with { ChildEvents = tree.ChildEvents.Add(log) };
    }

    [MustUseReturnValue]
    public GameEventLogTree AddDirectChildren(IEnumerable<GameEventLogTree> logs)
    {
        return logs.Aggregate(AsTree(), (current, next) => current.AddDirectChild(next));
    }

    [MustUseReturnValue]
    public abstract GameEventLogTree AsTree();

    [MustUseReturnValue]
    public GameEventLogTree Add(ILoggableGameEvent @event)
    {
        return Add(@event.Log());
    }

    [MustUseReturnValue]
    public GameEventLogTree MaybeAdd(ILoggableGameEvent? @event)
    {
        return MaybeAdd(@event?.Log());
    }

    [MustUseReturnValue]
    public GameEventLogTree MaybeAdd(GameEventLogTree? log)
    {
        return log is null ? AsTree() : Add(log);
    }
}
