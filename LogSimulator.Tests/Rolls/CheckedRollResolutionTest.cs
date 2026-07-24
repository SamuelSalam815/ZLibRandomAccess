using System;
using System.Linq;
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

    private static void AssertThrows(Action action)
    {
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(3)]
    [DataRow(5)]
    public void ResolvingWithTheIncorrectNumberOfDice_ShouldThrow(int numberOfRolls)
    {
        var rolls = Enumerable.Repeat(2, numberOfRolls).ToArray();
        AssertThrows(() => CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .WithDisadvantage(1)
            .AgainstDifficulty(10).ResolveWith(rolls));
    }

    [TestMethod]
    [DataRow(new[]{1,2,3,5})]
    [DataRow(new[]{1,2,-3,4})]
    [DataRow(new[]{1,2,0,4})]
    public void ResolvingWithAnyInvalidRolls_ShouldThrow(int[] rolls)
    {
        AssertThrows(() => CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .WithDisadvantage(1)
            .AgainstDifficulty(10).ResolveWith(rolls));
    }

    [TestMethod]
    [DataRow(new[] { 4, 1, 2, 4 }, 10, 1)]
    [DataRow(new[] { 3, 3, 4, 1 }, 10, 1)]
    [DataRow(new[] { 4, 3, 4}, 10, 0)]
    [DataRow(new[] { 4, 3, 4, 4}, 10, -1)]
    [DataRow(new[] { 2, 3, 2, 2}, 6, -1)]
    [DataRow(new[] { 2, 1, 2, 2}, 5, -1)]
    public void RollingAboveTheDifficulty_ShouldSucceed(int[] rolls, int difficulty, int advantage)
    {
        CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .AgainstDifficulty(difficulty)
            .WithAdvantage(advantage)
            .ResolveWith(rolls)
            .IsSuccess()
            .ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(new[] { 3, 1, 2, 4 }, 10, 1)]
    [DataRow(new[] { 3, 2, 4, 1 }, 10, 1)]
    [DataRow(new[] { 4, 3, 2}, 10, 0)]
    [DataRow(new[] { 4, 2, 3, 4}, 10, -1)]
    public void RollingBelowTheDifficulty_ShouldFail(int[] rolls, int difficulty, int advantage)
    {
        CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .AgainstDifficulty(difficulty)
            .WithAdvantage(advantage)
            .ResolveWith(rolls)
            .IsSuccess()
            .ShouldBeFalse();
    }

    [TestMethod]
    public void ResolvingWithDisadvantage_ShouldDropTheHighestValues()
    {
        var resolution = CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .WithDisadvantage(1)
            .AgainstDifficulty(10)
            .WithDisadvantage(2)
            .ResolveWith(3, 2, 1, 4, 4);

        resolution.RollsSelected.ShouldBe([3,2,1]);
        resolution.RollsDiscarded.ShouldBe([4,4]);
        resolution.RolledTotal.ShouldBe(6);
    }

    [TestMethod]
    public void ResolvingWithAdvantage_ShouldDropTheLowestValues()
    {
        var resolution = CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .WithDisadvantage(1)
            .AgainstDifficulty(10)
            .WithAdvantage(2)
            .ResolveWith(1, 1, 4, 4, 4);

        resolution.RollsSelected.ShouldBe([4,4,4]);
        resolution.RollsDiscarded.ShouldBe([1,1]);
        resolution.RolledTotal.ShouldBe(12);
    }

    [TestMethod]
    [DataRow(2, new []{4,2,3,1,4}, new[]{4,3,4}, new []{2,1})]
    [DataRow(-2, new []{4,2,3,1,3}, new[]{2,3,1}, new []{4,3})]
    public void Rolls_ShouldHaveTheirOrderPreserved(
        int advantage,
        int[] rolls,
        int[] expectedRollsSelected,
        int[] expectedRollsDiscarded)
    {
        var resolution = CheckedRollBuilder
            .Test(TestQuestion)
            .Roll(3)
            .D(4)
            .AgainstDifficulty(10)
            .WithAdvantage(advantage)
            .ResolveWith(rolls);

        resolution.RollsSelected.ShouldBe(expectedRollsSelected);
        resolution.RollsDiscarded.ShouldBe(expectedRollsDiscarded);
    }
}
