using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(CheckedRollResolution))]
public class CheckedRollResolutionTest
{
    private const string TestQuestion = "Will I rewrite this code?";

    [TestMethod]
    [DataRow(new[] { 4, 1, 2, 4 }, 10, 1)]
    [DataRow(new[] { 3, 3, 4, 1 }, 10, 1)]
    [DataRow(new[] { 4, 3, 4}, 10, 0)]
    [DataRow(new[] { 4, 3, 4, 4}, 10, -1)]
    [DataRow(new[] { 2, 3, 2, 2}, 6, -1)]
    [DataRow(new[] { 2, 1, 2, 2}, 5, -1)]
    public void RollingAboveTheDifficulty_ShouldSucceed(int[] rolls, int difficulty, int advantage)
    {
        RollBuilder
            .Roll(3)
            .D(4)
            .WithAdvantage(advantage)
            .AgainstDifficulty(difficulty, TestQuestion)
            .ResolveWith(rolls).IsSuccess
            .ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(new[] { 3, 1, 2, 4 }, 10, 1)]
    [DataRow(new[] { 3, 2, 4, 1 }, 10, 1)]
    [DataRow(new[] { 4, 3, 2}, 10, 0)]
    [DataRow(new[] { 4, 2, 3, 4}, 10, -1)]
    public void RollingBelowTheDifficulty_ShouldFail(int[] rolls, int difficulty, int advantage)
    {
        RollBuilder
            .Roll(3)
            .D(4)
            .WithAdvantage(advantage)
            .AgainstDifficulty(difficulty, TestQuestion)
            .ResolveWith(rolls).IsSuccess
            .ShouldBeFalse();
    }
}
