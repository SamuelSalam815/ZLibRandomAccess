using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace LogSimulator.Logging;

public record GameEventLog(EventScope EventScope, string Description, bool IsSummary) : GameEventLogger
{
    [MustUseReturnValue]
    public static GameEventLog Log(
        EventScope eventScope,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return new GameEventLog(eventScope, string.Format(log, args), false);
    }

    [MustUseReturnValue]
    public GameEventLogTree FlagAsSummary() => this with { IsSummary = true };

    [MustUseReturnValue]
    public static GameEventLog GlobalEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.Global, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog GameEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.Game, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog GamePhaseEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.GamePhase, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog TurnEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.Turn, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog CombatActionEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.CombatAction, log, args);
    }

    [MustUseReturnValue]
    public static GameEventLog RollEvent(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string log,
        params object?[] args)
    {
        return Log(EventScope.Roll, log, args);
    }

    public static implicit operator GameEventLogTree(GameEventLog log)
    {
        return new GameEventLogTree(log, []);
    }

    [MustUseReturnValue]
    public override GameEventLogTree Add(GameEventLogTree newLogTree)
    {
        return ((GameEventLogTree)this).Add(newLogTree);
    }

    public override GameEventLogTree AsTree() => this;

    public override string ToString()
    {
        return $"[{EventScope.ToString().ToUpper()}] {Description}";
    }
}
