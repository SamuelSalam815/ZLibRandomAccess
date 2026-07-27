using System;
using System.Collections.Generic;
using System.Linq;
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
    private Character TestHero() => new("_JimBob_", new StatBlock(6, 5, 4));

    [TestMethod]
    [DataRow(3, 6)]
    [DataRow(4, 2)]
    public void FailingPeacefulRoamingCheck_CausesCombat(int firstRoll, int secondRoll)
    {
        new OverworldPhase(TestHero())
            .ProgressGame(DieRollBuilder.Provide(firstRoll, secondRoll))
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldBeOfType<CombatPhase>();
    }

    [TestMethod]
    [DataRow(5,5)]
    [DataRow(6,6)]
    public void PassingPeacfulRoamingCheck_StaysInTheOverworld(int firstRoll, int secondRoll)
    {
        new OverworldPhase(TestHero())
            .ProgressGame(DieRollBuilder.Provide(firstRoll, secondRoll))
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldBeOfType<OverworldPhase>();
    }

    [TestMethod]
    public void TestLogger()
    {
        var eventDescriptions = new List<GameEventDescription>();
        var random = new Random();

        GamePhase currentGamePhase = new OverworldPhase(TestHero());
        GameProgress progress;
        do
        {
            progress = currentGamePhase.ProgressGame(diceSize => random.Next(1, diceSize));
            eventDescriptions.AddRange(progress.GameEvents.Select(e => e.DescribeEvent()));

            if (progress.NextGamePhase is { } nextGamePhase)
            {
                currentGamePhase = nextGamePhase;
            }
        } while (progress.NextGamePhase is not null);


        ;
    }
}
