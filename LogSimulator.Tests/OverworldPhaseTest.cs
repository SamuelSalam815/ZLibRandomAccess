using System;
using JetBrains.Annotations;
using LogSimulator.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests;

[TestClass]
[TestSubject(typeof(OverworldPhase))]
public class OverworldPhaseTest
{
    [TestMethod]
    [DataRow(3, 6)]
    [DataRow(4, 2)]
    public void FailingSurvivalCheck_CausesGameOver(int firstRoll, int secondRoll)
    {
        new OverworldPhase()
            .ProgressGame(DieRollBuilder.Provide(firstRoll, secondRoll))
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldBeOfType<GameOverPhase>();
    }

    [TestMethod]
    [DataRow(5,5)]
    [DataRow(6,6)]
    public void PassingSurvivalCheck_ContinuesTheGame(int firstRoll, int secondRoll)
    {
        new OverworldPhase()
            .ProgressGame(DieRollBuilder.Provide(firstRoll, secondRoll))
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldNotBeOfType<GameOverPhase>();
    }

    [TestMethod]
    public void TestLogger()
    {
        var logger = new GameEventLogger();
        var random = new Random();

        var progress = new OverworldPhase().ProgressGame(diceSize => random.Next(1, diceSize));

        foreach (var @event in progress.GameEvents)
        {
            @event.LogEvent(logger);
        }

        ;
    }
}
