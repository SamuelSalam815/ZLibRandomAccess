using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace LogSimulator.Logging;

public record GameEventLog(EventLevel EventLevel, string Description) : GameEventLogger
{
    [MustUseReturnValue]
    public static GameEventLog Log(
        EventLevel eventLevel,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return new GameEventLog(eventLevel, string.Format(log, args));
    }

    [MustUseReturnValue]
    public static GameEventLog GlobalEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.Global, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog GameEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.Game, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog GamePhaseEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.GamePhase, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog TurnEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.Turn, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog ActionEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.Action, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog RollEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventLevel.Roll, log, args);
    }

    public static implicit operator GameEventLogTree(GameEventLog log)
    {
        return new GameEventLogTree(log, []);
    }

    [MustUseReturnValue]
    public override GameEventLogTree Add(GameEventLogTree other)
    {
        return ((GameEventLogTree)this).Add(other);
    }

    public override GameEventLogTree AsTree() => this;

    public override string ToString()
    {
        return $"[{EventLevel.ToString().ToUpper()}] {Description}";
    }
}
