using System;
using System.Collections.Generic;
using ArchiveAccessPointVisualizer;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(PeriodicProgressLogic<>))]
public class PeriodicProgressLogicTest
{
    private readonly DateTime _testTime = new(2026, 09, 10, 12, 00, 00);

    private readonly TimeSpan _progressCheckInterval = TimeSpan.FromSeconds(5);

    private PeriodicProgressLogic<int> CreateSut(Func<ProgressReport<int>> progressCheck)
    {
        return new PeriodicProgressLogic<int>(
            _progressCheckInterval,
            progressCheck,
            _testTime,
            0);
    }

    private PeriodicProgressLogic<int> CreateSut(IEnumerable<ProgressReport<int>> progressCheck)
    {
        return new PeriodicProgressLogic<int>(
            _progressCheckInterval,
            EnumerateProgressChecks(progressCheck),
            _testTime,
            0);
    }

    private static Func<ProgressReport<T>> EnumerateProgressChecks<T>(IEnumerable<ProgressReport<T>> reports)
    {
        var enumerator = reports.GetEnumerator();
        bool isComplete;
        return () =>
        {
            isComplete = !enumerator.MoveNext();
            if (isComplete)
            {
                throw new InvalidOperationException("Ran out of predefined progress checks!");
            }

            var result = enumerator.Current;

            if (isComplete)
            {
                enumerator.Dispose();
            }
            return result;
        };
    }

    [TestMethod]
    public void FirstTick_ShouldMakeAReport()
    {
        var periodicProgressLogic = CreateSut(() => 10)
            .Update(_testTime);
        periodicProgressLogic.CurrentProgress.ShouldBe(10);
        periodicProgressLogic.IsTerminated.ShouldBeFalse();
    }

    [TestMethod]
    public void ReturningTheFinalReport_ShouldTerminateLogic()
    {
        CreateSut(() => new ProgressReport<int>(13, true))
            .Update(_testTime)
            .IsTerminated
            .ShouldBeTrue();
    }

    [TestMethod]
    public void NextProgressCheck_IsDelayedByCheckInterval()
    {
        var sut = CreateSut(() => 14);
        sut.MinimumDelayBeforeNextReport.ShouldBe(TimeSpan.Zero);
        sut = sut.Update(_testTime);
        sut.MinimumDelayBeforeNextReport.ShouldBe(_progressCheckInterval);
    }

    [TestMethod]
    public void CurrentTime_IsUpdatedOnUpdate()
    {
        var newTime = _testTime.AddTicks(100);
        var sut = CreateSut(() => 15);
        sut.CurrentTime.ShouldBe(_testTime);
        sut.Update(newTime).CurrentTime.ShouldBe(newTime);
    }

    [TestMethod]
    public void MinimumDelayIsUpdated_WhenNotEnoughTimeHasPassedForTheNectCheck()
    {
        var timeDelay = TimeSpan.FromSeconds(2);
        var expectedRemainingTime = _progressCheckInterval.Subtract(timeDelay);
        var newTime = _testTime.Add(timeDelay);
        var sut = CreateSut([16]).Update(_testTime);
        sut.MinimumDelayBeforeNextReport.ShouldBe(_progressCheckInterval);
        sut = sut.Update(newTime);

        sut.MinimumDelayBeforeNextReport.ShouldBe(expectedRemainingTime);
    }

    [TestMethod]
    public void ProgressIsUpdated_WhenUpdateIsCalled()
    {
        CreateSut([17]).Update(_testTime).CurrentProgress.ShouldBe(17);
    }

    [TestMethod]
    public void ProgressIsUpdated_WhenUpdatesAreSpacedProperlyInTime()
    {
        CreateSut([5, 10, 18])
            .Update(_testTime)
            .Update(_testTime + _progressCheckInterval)
            .Update(_testTime + _progressCheckInterval * 2)
            .CurrentProgress
            .ShouldBe(18);
    }

    [TestMethod]
    public void TerminatedLogic_DoesNoOpWhenUpdated()
    {
        var sut = CreateSut([new ProgressReport<int>(19, true)]).Update(_testTime);
        sut.IsTerminated.ShouldBeTrue();
        var updatedSut = sut.Update(_testTime.AddTicks(50));
        updatedSut.ShouldBe(sut);
    }

}
