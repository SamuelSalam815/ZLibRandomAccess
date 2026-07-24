using JetBrains.Annotations;
using LogSimulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests;

[TestClass]
[TestSubject(typeof(Test))]
public class TestTest
{

    [TestMethod]
    [DataRow(10, 10)]
    [DataRow(11, 10)]
    [DataRow(12, 10)]
    public void MeetingTheTestDifficulty_ShouldBeatTheTest(int roll, int difficulty)
    {
        new Test("Will the test result be expected?", difficulty).AttemptWith(roll, out _).ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(1, 10)]
    [DataRow(5, 10)]
    [DataRow(9, 10)]
    public void RollingBelowTheTestDifficulty_ShouldFailTheTest(int roll, int difficulty)
    {
        new Test("Will the test fail?", difficulty).AttemptWith(roll, out _).ShouldBeFalse();
    }
}
