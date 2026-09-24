using ArchiveAccessPointVisualizer;
using ErrorOr;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(LogFileCompressionJobStatus))]
public class LogFileCompressionJobStatusTest
{

    [TestMethod]
    public void AcceptingProgress_CopiesByteReport()
    {
        var sut = LogFileCompressionJobStatus.NotReady;
        var report = new LogFileCompressionProgressReport(123, null);
        sut.NumberOfBytesWritten.ShouldNotBe(report.TotalNumberOfBytesWritten);
        sut.Accept(report).NumberOfBytesWritten.ShouldBe(report.TotalNumberOfBytesWritten);
    }

    [TestMethod]
    public void DescriptionIsNotUpdated_IfProgressDescriptionIsNull()
    {
        const string description = "Foo bar";
        var sut = LogFileCompressionJobStatus.NotReady with { JobStatusDescription = description };
        sut.Accept(new LogFileCompressionProgressReport(100, null)).JobStatusDescription.ShouldBe(description);
    }

    [TestMethod]
    public void ReportingJobComplete_ShouldChangeJobToNotRunning()
    {
        new LogFileCompressionJobStatus(
            true,
            0,1,
            "Running...")
            .Accept(new LogFileCompressionProgressReport(1, "Completed!", true))
            .IsRunning.ShouldBeFalse();
    }

    [TestMethod]
    public void ReportingAnError_ShouldChangeJobToNotRunning()
    {
        const string errorMessage = "Did not have the required permissions to write that file";
        var sut = new LogFileCompressionJobStatus(
                true,
                0,1,
                "Running...")
            .Accept(Error.Unauthorized(description: errorMessage));

        sut.IsRunning.ShouldBeFalse();
        sut.JobStatusDescription.ShouldBe(errorMessage);
    }

    [TestMethod]
    public void DescriptionIsUpdated_IfProgressDescriptionIsNotNull()
    {
        const string description = "Foo bar";
        var sut = LogFileCompressionJobStatus.NotReady;
        sut.JobStatusDescription.ShouldNotBe(description);
        sut.Accept(new LogFileCompressionProgressReport(100, description)).JobStatusDescription.ShouldBe(description);
    }
}
