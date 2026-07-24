using JetBrains.Annotations;
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
}
