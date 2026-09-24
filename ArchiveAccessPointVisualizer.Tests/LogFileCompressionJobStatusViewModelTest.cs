using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(LogFileCompressionJobStatusViewModel))]
public class LogFileCompressionJobStatusViewModelTest
{

    public static IEnumerable<object?[]> StartButtonTestCases()
    {
        yield return [
            new LogFileCompressionJobStatus(false, 100, 100, "Done"), LogFileCompressionJobStatusViewModel.StartButtonLabelString
        ];

        yield return [
            new LogFileCompressionJobStatus(true, 100, 1000, "Foo Bar"),
            "10.00%"
        ];

        yield return [
            new LogFileCompressionJobStatus(true, 9_786, 10_000, "Foo Bar"),
            "97.86%"
        ];

        yield return [
            new LogFileCompressionJobStatus(true, 97_865, 100_000, "Foo Bar"),
            "97.87%"
        ];
    }

    [TestMethod]
    [DynamicData(nameof(StartButtonTestCases))]
    public void StartButtonText_IsDerivedCorrectlyFromLatestStatus(LogFileCompressionJobStatus status,
        string expectedText)
    {
        var sut = new LogFileCompressionJobStatusViewModel();
        sut.UpdateModel(status);
        sut.BeginCompressionJobButtonLabelOrProgressString.ShouldBe(expectedText);
    }
}
