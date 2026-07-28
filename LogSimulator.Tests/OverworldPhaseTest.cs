using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests;

[TestClass]
[TestSubject(typeof(OverworldPhase))]
public class OverworldPhaseTest
{
    private Character TestHero() => new("_JimBob_", new StatBlock(6, 5, 5));

    private GameState TestGameState() => new(TestHero(), GameEventLog.GlobalEvent("Game event log initialized!"));

    [TestMethod]
    [DataRow(new[] { 3, 4})]
    [DataRow(new[] { 4, 2})]
    public void FailingPeacefulRoamingCheck_CausesCombat(int[] rollsToProvide)
    {
        new OverworldPhase(TestGameState())
            .ProgressGame(DieRollBuilder.Provide(rollsToProvide).ThenRepeat(1))
            .ShouldNotBeNull()
            .ShouldBeOfType<CombatPhase>();
    }

    [TestMethod]
    [DataRow(new[] { 4, 5})]
    [DataRow(new[] { 6, 5})]
    public void PassingPeacefulRoamingCheck_StaysInTheOverworld(int[] rollsToProvide)
    {
        new OverworldPhase(TestGameState())
            .ProgressGame(DieRollBuilder.Provide(rollsToProvide).ThenRepeat(1))
            .ShouldNotBeNull()
            .ShouldBeOfType<OverworldPhase>();
    }

    [TestMethod]
    // [Ignore]
    public void SimulateGame()
    {
        GameEventLogTree? globalEventLog = null;
        var terminalGamePhases  = new List<GamePhase>();
        var random = new Random();

        GamePhase currentGamePhase;
        do
        {
            var newGame = TestGameState();
            if (globalEventLog != null)
            {
                newGame = newGame with { GameEventLog = globalEventLog };
            }
            currentGamePhase = OverworldPhase.NewGame(newGame);
            GamePhase? nextGamePhase;
            do
            {
                nextGamePhase = currentGamePhase.ProgressGame(diceSize => random.Next(1, diceSize + 1));

                if (nextGamePhase is not null)
                {
                    currentGamePhase = nextGamePhase;
                }
            } while (nextGamePhase is not null);

            terminalGamePhases.Add(currentGamePhase);
            globalEventLog = currentGamePhase.GameState.GameEventLog;
        } while (currentGamePhase.GameState is {NumberOfLimitBreaks: <= 1 });

        ;
    }
}
