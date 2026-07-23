using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using TestResult = LogSimulator.Rolls.TestResult;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(TestResult))]
public class TestResultTest
{
    [TestMethod]
    [DataRow(10, 10)]
    [DataRow(10, 11)]
    [DataRow(10, 12)]
    [DataRow(8, 8)]
    [DataRow(8, 9)]
    public void TestPasses_WhenRollMeetsOrBeatsDifficulty(int testDifficulty, int roll)
    {
        RollRequest
            .Roll(1)
            .D(roll)
            .WithDifficulty(testDifficulty)
            .ResolveWith([roll])
            .IsPassed
            .ShouldBeTrue();
    }
}
