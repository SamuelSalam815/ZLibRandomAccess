using System;
using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(RollRequest))]
public class RollRequestTest
{

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(7)]
    public void ResolvingWithAnImpossibleRoll_ThrowsException(int rollValue)
    {
        Action action = () => RollRequest.Roll(1).ResolveWith([rollValue]);
        action.ShouldThrow<ArgumentException>();
    }

    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(1, 2)]
    public void ResolvingWithIncorrectNumberOfRolls_ThrowsException(int requiredNumberOfRolls, int actualNumberOfRolls)
    {
        var rolls = Enumerable.Repeat(3, actualNumberOfRolls);
        Action action = () => RollRequest.Roll(requiredNumberOfRolls).ResolveWith(rolls);
        action.ShouldThrow<ArgumentException>();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(-1)]
    [DataRow(2)]
    public void Advantage_ShouldIncreaseRequiredRollCount(int advantage)
    {
        RollRequest.Roll(1).WithAdvantage(advantage).RequiredRollCount.ShouldBe(advantage + 1);
    }

    [TestMethod]
    public void ResolvingRequest_ShouldReturnResultThatReferencesTheRequest()
    {
        var request = RollRequest.Roll(3).D(4).Plus(3).WithAdvantage();
        var result = request.ResolveWith([3, 2, 4, 1]);
        result.RollRequest.ShouldBe(request);
    }
}
