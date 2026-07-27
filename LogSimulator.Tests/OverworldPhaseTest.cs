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

    private GameState TestGameState() => new(TestHero());

    [TestMethod]
    [DataRow(new[] { 3, 4})]
    [DataRow(new[] { 4, 2})]
    public void FailingPeacefulRoamingCheck_CausesCombat(int[] rollsToProvide)
    {
        new OverworldPhase(TestGameState())
            .ProgressGame(DieRollBuilder.Provide(rollsToProvide).ThenRepeat(1))
            .NextGamePhase
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
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldBeOfType<OverworldPhase>();
    }

    [TestMethod]
    [Ignore]
    public void SimulateGame()
    {
        var games = new List<List<GameEventDescription>>();
        var random = new Random();

        GamePhase currentGamePhase;
        do
        {
            currentGamePhase = new OverworldPhase(TestGameState());
            GameProgress progress;
            var eventDescriptions = new List<GameEventDescription>();
            do
            {
                progress = currentGamePhase.ProgressGame(diceSize => random.Next(1, diceSize));
                eventDescriptions.AddRange(progress.GameEvents.Select(e => e.DescribeEvent()));

                if (progress.NextGamePhase is { } nextGamePhase)
                {
                    currentGamePhase = nextGamePhase;
                }
            } while (progress.NextGamePhase is not null);

            games.Add(eventDescriptions);
        } while (currentGamePhase.GameState is not { FinalBossDefeated: true, NumberOfLimitBreaks: 2 });

        ;
    }
}
