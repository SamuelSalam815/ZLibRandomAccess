using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests;

[TestClass]
[TestSubject(typeof(Rotation<>))]
public class RotationTest
{
    [TestMethod]
    public void CyclingThroughARotation_ShouldRepeatASequence()
    {
        var baseList = new List<int>{ 1, 3, 1, 2 };
        var sut = new Rotation<int>(baseList);
        foreach (var expected in Enumerable.Repeat(baseList, 3).SelectMany(i => i))
        {
            sut = sut.Cycle(out var actual);
            actual.ShouldBe(expected);
        }
    }

    [TestMethod]
    [DataRow(0, new[]{3, 4, 5}, new int[]{}, 0)]
    [DataRow(1, new[]{3, 4, 5}, new []{3}, 1)]
    [DataRow(2, new[]{3, 4, 5}, new []{3,4}, 2)]
    [DataRow(3, new[]{3, 4, 5}, new []{3,4,5}, 0)]
    [DataRow(4, new[]{3, 4, 5}, new []{3,4,5,3}, 1)]
    [DataRow(-1, new[]{3, 4, 5}, new []{3}, 2)]
    [DataRow(-2, new[]{3, 4, 5}, new []{3,5}, 1)]
    [DataRow(-3, new[]{3, 4, 5}, new []{3,5,4}, 0)]
    [DataRow(-4, new[]{3, 4, 5}, new []{3,5,4,3}, 2)]
    public void CyclingMultipleTimes_ShouldProduceTheExpectedSequence(
        int cycleCount,
        int[] baseSequence,
        int[] expectedSequence,
        int expectedFinalIndex)
    {
        new Rotation<int>(baseSequence).Cycle(cycleCount, out var actualSequence);
        actualSequence.ShouldBe(expectedSequence);
    }

    [TestMethod]
    public void ListRepresentation_ShouldRotateWithEachCycle()
    {
        var baseItems = new[] { 2, 8, 1, 5 };
        var expectedArrays = new[]
        {
            new[] { 8, 1, 5, 2 },
            new[] { 1, 5, 2, 8 },
            new[] { 5, 2, 8, 1 },
            new[] { 2, 8, 1, 5 }
        };

        var sut = new Rotation<int>(baseItems);
        sut.ToArray().ShouldBe(baseItems);

        foreach (var expectedList in expectedArrays)
        {
            sut = sut.Cycle();
            sut.ToArray().ShouldBe(expectedList);
        }
    }
}
