using System;
using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Checks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Checks;

[TestClass]
[TestSubject(typeof(AbilityCheckResolution.ByRolling))]
public class ByRollingTest
{
    private AbilityCheckBuilder BuildTestAbilityCheck()
    {
        return AbilityCheckBuilder
            .Test("Will I rewrite this code?")
            .Roll(3)
            .D(4)
            .With(new AdvantageRating(-1))
            .AgainstDifficulty(10);
    }

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
        AssertThrows(() => AbilityCheckResolution.ByRolling.CreateFrom(BuildTestAbilityCheck(), rolls));
    }

    [TestMethod]
    [DataRow(new[]{1,2,3,5})]
    [DataRow(new[]{1,2,-3,4})]
    [DataRow(new[]{1,2,0,4})]
    public void ResolvingWithFaceValuesOutOfAllowedValues_ShouldThrow(int[] rolls)
    {
        AssertThrows(() => AbilityCheckResolution.ByRolling.CreateFrom(BuildTestAbilityCheck(), rolls));
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
        var abilityCheck = BuildTestAbilityCheck()
            .AgainstDifficulty(difficulty)
            .WithAdvantage(advantage);
        AbilityCheckResolution.ByRolling.CreateFrom(abilityCheck, rolls).IsSuccess().ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(new[] { 3, 1, 2, 4 }, 10, 1)]
    [DataRow(new[] { 3, 2, 4, 1 }, 10, 1)]
    [DataRow(new[] { 4, 3, 2}, 10, 0)]
    [DataRow(new[] { 4, 2, 3, 4}, 10, -1)]
    public void RollingBelowTheDifficulty_ShouldFail(int[] rolls, int difficulty, int advantage)
    {
        var abilityCheck = BuildTestAbilityCheck()
            .AgainstDifficulty(difficulty)
            .WithAdvantage(advantage);
        AbilityCheckResolution.ByRolling.CreateFrom(abilityCheck, rolls).IsSuccess().ShouldBeFalse();
    }

    [TestMethod]
    public void ResolvingWithDisadvantage_ShouldDropTheHighestValues()
    {
        var abilityCheck = BuildTestAbilityCheck().WithDisadvantage(2);
        var resolution = AbilityCheckResolution.ByRolling.CreateFrom(abilityCheck, [3, 2, 1, 4, 4]);

        resolution.RollsSelected.ShouldAllBe(roll => roll != 4);
        resolution.RollsDiscarded.ShouldAllBe(roll => roll == 4);
        resolution.RolledTotal.ShouldBe(6);
    }

    [TestMethod]
    public void ResolvingWithAdvantage_ShouldDropTheLowestValues()
    {
        var abilityCheck = BuildTestAbilityCheck().WithAdvantage(2);
        var resolution = AbilityCheckResolution.ByRolling.CreateFrom(abilityCheck, [1, 1, 4, 4, 4]);

        resolution.RollsSelected.ShouldAllBe(roll => roll == 4);
        resolution.RollsDiscarded.ShouldAllBe(roll => roll == 1);
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
        var abilityCheck = BuildTestAbilityCheck().WithAdvantage(advantage);
        var resolution = AbilityCheckResolution.ByRolling.CreateFrom(abilityCheck, rolls);

        resolution.RollsSelected.ShouldBe(expectedRollsSelected);
        resolution.RollsDiscarded.ShouldBe(expectedRollsDiscarded);
    }
}
