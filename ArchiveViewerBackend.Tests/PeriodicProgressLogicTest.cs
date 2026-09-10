using System;
using ArchiveAccessPointVisualizer;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(PeriodicProgressLogic<>))]
public class PeriodicProgressLogicTest
{
    [TestMethod]
    public void FirstTick_ShouldMakeAReport()
    {
        var currentTime = new DateTime(2026, 09, 10, 12, 00, 00);
        var initialProgress = 0;
        var expectedProgress = 10;
        new PeriodicProgressLogic<int>(
            TimeSpan.FromSeconds(5),
            () => new ProgressReport<int>(expectedProgress, false),
            currentTime,
            initialProgress)
            .ProgressIfNeeded(currentTime)
            .CurrentProgress.ShouldBe(10);
    }
}
