using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LogSimulator.ChacterSpec;
using LogSimulator.Combat;
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
}
