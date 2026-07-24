using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(DiceRollResult))]
public class DiceRollResultTest
{
    private void AssertArgumentException(Action action)
    {
        action.ShouldThrow<ArgumentException>();
    }

    [TestMethod]
    [DataRow(2, 0)]
    [DataRow(2, -1)]
    [DataRow(2, 3)]
    public void ProvidingInvalidRollValues_ShouldThrow(int dieSize, int rolledValue)
    {
        AssertArgumentException(() => RollBuilder.Roll(1).D(dieSize).Create().ResolveWith([rolledValue]));
    }

    [TestMethod]
    [DataRow(2, 0)]
    [DataRow(2, 1)]
    [DataRow(2, 3)]
    public void ProvidingIncorrectNumberOfRolls_ShouldThrow(int requestedNumberOfRolls, int actualNumberOfRolls)
    {
        AssertArgumentException(() => RollBuilder
            .Roll(requestedNumberOfRolls)
            .Create()
            .ResolveWith(Enumerable.Repeat(2, actualNumberOfRolls).ToArray()));
    }

    public static IEnumerable<object[]> ReplacementRollTestData()
    {
        yield return [new ExtraRollRequest.Advantage(1), 0];
        yield return [new ExtraRollRequest.Advantage(1), 2];
        yield return [new ExtraRollRequest.Disadvantage(1), 0];
        yield return [new ExtraRollRequest.Disadvantage(1), 2];
    }

    [TestMethod]
    [DynamicData(nameof(ReplacementRollTestData))]
    public void ProvidingIncorrectNumberOfRollsWhenExtraRollsAreRequested_ShouldThrow(ExtraRollRequest extraRollRequest, int actualNumberOfReplacements)
    {
        AssertArgumentException(() =>
            RollBuilder.Roll(1)
            .With(extraRollRequest)
            .Create()
            .ResolveWith(
                Enumerable.Repeat(6, 1 + actualNumberOfReplacements).ToArray()
            )
        );
    }

    [TestMethod]
    public void ProvidingValidDieRolls_ShouldCreateAnExpectedResult()
    {
        RollBuilder
            .Roll(2)
            .D(3)
            .Create()
            .ResolveWith([3, 2])
            .RollTotal
            .ShouldBe(5);
    }

    [TestMethod]
    public void AdvantagedRolls_ShouldDiscardTheLowestValues()
    {
        RollBuilder
            .Roll(2)
            .D(3)
            .WithAdvantage(2)
            .Create()
            .ResolveWith([3, 2, 1, 1])
            .RollTotal
            .ShouldBe(5);
    }

    [TestMethod]
    public void DisadvantagedRolls_ShouldDiscardTheHighestValues()
    {
        RollBuilder
            .Roll(2)
            .D(3)
            .WithDisadvantage(2)
            .Create()
            .ResolveWith([3, 2, 1, 1])
            .RollTotal
            .ShouldBe(2);
    }
}
