using System;
using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Logging;

[TestClass]
[TestSubject(typeof(GameEventLogTree))]
public class GameEventLogTreeTest
{

    [TestMethod]
    public void StringRepresentation_ShouldIndentSubEvents()
    {
        GameEventLog
            .GameEvent("Hello, World!")
            .AddDirectChild(GameEventLog.GameEvent("Foo"))
            .AddDirectChild(
                GameEventLog.GameEvent("Fizz")
                    .AddDirectChild(GameEventLog.GameEvent("Buzz"))
                    .AddDirectChild(GameEventLog.GameEvent("Buzz")))
            .AddDirectChild(GameEventLog.GameEvent("Bar"))
            .ToString()
            .ShouldBe("""
                       [GAME] Hello, World!
                       |   [GAME] Foo
                       |   [GAME] Fizz
                       |   |   [GAME] Buzz
                       |   |   [GAME] Buzz
                       |   [GAME] Bar

                       """
            );
    }

    [TestMethod]
    [DataRow(EventScope.Global, EventScope.Global)]
    [DataRow(EventScope.Game, EventScope.Global)]
    [DataRow(EventScope.Game, EventScope.Game)]
    [DataRow(EventScope.GamePhase, EventScope.Game)]
    [DataRow(EventScope.GamePhase, EventScope.GamePhase)]
    [DataRow(EventScope.Turn, EventScope.GamePhase)]
    [DataRow(EventScope.Turn, EventScope.Turn)]
    [DataRow(EventScope.CombatAction, EventScope.Turn)]
    [DataRow(EventScope.CombatAction, EventScope.CombatAction)]
    [DataRow(EventScope.Roll, EventScope.CombatAction)]
    [DataRow(EventScope.Roll, EventScope.Roll)]
    public void AddingTheSameOrWiderScopedLogThanTheRootLog_ShouldThrow(EventScope rootLogScope, EventScope addedEventScope)
    {
        var action = () => GameEventLog
            .Log(rootLogScope, "Existing Log")
            .Add(GameEventLog.Log(addedEventScope, "New Log"));

        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void AddingToLogWithNoChildren_ShouldAddNewLogToChildren()
    {
        var newLog = GameEventLog.TurnEvent("Turn Over!");

        GameEventLog
            .GlobalEvent("Initial Log!")
            .Add(newLog)
            .ChildEvents
            .ShouldBe([newLog]);
    }

    [TestMethod]
    [DataRow(EventScope.Game, EventScope.Game)]
    [DataRow(EventScope.GamePhase, EventScope.Game)]
    [DataRow(EventScope.GamePhase, EventScope.GamePhase)]
    [DataRow(EventScope.Turn, EventScope.GamePhase)]
    [DataRow(EventScope.Turn, EventScope.Turn)]
    [DataRow(EventScope.CombatAction, EventScope.Turn)]
    [DataRow(EventScope.CombatAction, EventScope.CombatAction)]
    [DataRow(EventScope.Roll, EventScope.CombatAction)]
    [DataRow(EventScope.Roll, EventScope.Roll)]
    public void MaintainingOrWideningEventScope_ShouldAddNewLogAsSibling(EventScope predecessorScope, EventScope addedEventScope)
    {
        var logToAdd = GameEventLog.Log(addedEventScope, "Added Log");

        GameEventLog
            .GlobalEvent("InitialLog")
            .Add(GameEventLog.Log(predecessorScope, "Predecessor"))
            .Add(logToAdd)
            .ChildEvents
            .Last()
            .ShouldBe(logToAdd);
    }

    [TestMethod]
    public void AddingToASummaryLog_ShouldThrow()
    {
        var logToAdd = GameEventLog.CombatActionEvent("Strike Again!");

        var action = () => GameEventLog.TurnEvent("Turn Over!").FlagAsSummary().Add(logToAdd);

        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void AddingLogWithNarrowerScope_ShouldIncreaseNestingLevel()
    {
        var innerLog = GameEventLog.GamePhaseEvent("Day Phase Begin!");
        var outerLog = GameEventLog.GameEvent("Game Begin!").Add(innerLog);
        var logToAdd = GameEventLog.TurnEvent("Turn Begin");

        var expectedNesting = innerLog.Add(logToAdd);

        outerLog.Add(logToAdd).ChildEvents.ShouldBe([expectedNesting]);
    }

    [TestMethod]
    public void AddingSummaryLog_ShouldIntegrateRecentLowerLevelLogsAsChildren()
    {
        var dayLog = GameEventLog.GamePhaseEvent("Day Begin")
            .Add(GameEventLog.TurnEvent("Roamed the Overworld"))
            .Add(GameEventLog.TurnEvent("Roamed the Overworld"))
            .Add(GameEventLog.TurnEvent("Combat initiated!"));

        var globalLog = GameEventLog
            .GlobalEvent("Initial Log!")
            .Add(dayLog);

        var summaryLog = GameEventLog.GamePhaseEvent("Roamed the Overworld twice before combat was initiated!")
            .FlagAsSummary();

        var expectedNesting = summaryLog.AddDirectChildren(dayLog.ChildEvents);

        var updatedGlobalLog = globalLog.Add(summaryLog);

        updatedGlobalLog.ChildEvents.ShouldBe([expectedNesting]);
    }

    [TestMethod]
    public void AddingSummaryLogWithChildren_ShouldThrow()
    {
        var dayLog = GameEventLog.GamePhaseEvent("Day Begin")
            .FlagAsSummary()
            .AddDirectChildren(
                [GameEventLog.TurnEvent("Roamed the Overworld"),
                GameEventLog.TurnEvent("Roamed the Overworld"),
                GameEventLog.TurnEvent("Combat initiated!")]
            );

        var action = () => GameEventLog.GlobalEvent("Initial Log!").Add(dayLog);

        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void AddingSummaryLogWithoutEquivalentScopeSibling_ShouldThrow()
    {
        var combatSummary =
            GameEventLog.GlobalEvent("Initial Log!")
                .Add(GameEventLog.TurnEvent("Took 0 damage in combat"))
                .Add(GameEventLog.TurnEvent("Took 3 damage in combat"))
                .Add(GameEventLog.TurnEvent("Took 8 damage in combat"));

        var action = () => combatSummary.Add(GameEventLog.GamePhaseEvent("Was defeated in combat after 3 rounds").FlagAsSummary());

        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void AddingSummaryLogWhenSiblingIsAlreadyASummary_ShouldThrow()
    {
        var combatSummaryLog = GameEventLog.GamePhaseEvent("Was defeated in combat after 3 rounds").FlagAsSummary();
        var combatLog =
            GameEventLog.GlobalEvent("Initial Log!")
                .Add(GameEventLog.GamePhaseEvent("Combat with Skeleton begun!"))
                .Add(GameEventLog.TurnEvent("Took 0 damage in combat"))
                .Add(GameEventLog.TurnEvent("Took 3 damage in combat"))
                .Add(GameEventLog.TurnEvent("Took 8 damage in combat"))
                .Add(combatSummaryLog);

        var action = () => combatLog.Add(combatSummaryLog);

        action.ShouldThrow<Exception>();
    }
}
