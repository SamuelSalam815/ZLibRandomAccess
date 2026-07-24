using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests;

[TestClass]
[TestSubject(typeof(OverworldPhase))]
public class OverworldPhaseTest
{
    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    public void FailingSurvivalCheck_CausesGameOver(int constantRollValue)
    {
        new OverworldPhase()
            .ProgressGame(_ => constantRollValue)
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldBeOfType<GameOverPhase>();
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(6)]
    public void PassingSurvivalCheck_ContinuesTheGame(int constantRollValue)
    {
        new OverworldPhase()
            .ProgressGame(_ => constantRollValue)
            .NextGamePhase
            .ShouldNotBeNull()
            .ShouldNotBeOfType<GameOverPhase>();

    }
}
