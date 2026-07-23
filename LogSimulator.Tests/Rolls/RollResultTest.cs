using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(RollResult))]
public class RollResultTest
{

    [TestMethod]
    public void Modifier_ShouldBeIncludedInResolvedValue()
    {
        RollRequest.Roll(1).D(4).Plus(3).ResolveWith([4]).ResolvedValue.ShouldBe(7);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public void Advantage_DiscardsLowestValues(int advantage)
    {
        const int dieCount = 3;
        var request = RollRequest.Roll(dieCount).D(4).WithAdvantage(advantage);
        var advantageRolls = Enumerable.Repeat(1, advantage);
        var rolls = Enumerable.Range(1, dieCount).Concat(advantageRolls);

        request.ResolveWith(rolls).ResolvedValue.ShouldBe(6);
    }
}
