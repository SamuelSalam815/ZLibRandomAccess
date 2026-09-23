using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(LogFileCompressionRequestBuilder))]
public class LogFileCompressionRequestBuilderTest
{
    [TestMethod]
    [DataRow(@"C:/temp/output.gz", @$"C:/temp/output{LogFileCompressionRequestBuilder.DefaultRecoveryPointFileExtension}")]
    [DataRow(@"C:/temp/output", @$"C:/temp/output{LogFileCompressionRequestBuilder.DefaultRecoveryPointFileExtension}")]
    [DataRow(@"foo", @$"foo{LogFileCompressionRequestBuilder.DefaultRecoveryPointFileExtension}")]
    public void SettingOutputFilePath_DerivesRecoveryPointFilePathAsExpected(
        string newOutputFilePath,
        string expectedDerivedPath)
    {
        new LogFileCompressionRequestBuilder
        {
            OutputFilePath = newOutputFilePath
        }.DerivedRecoveryPointFilePath.ShouldBe(expectedDerivedPath);
    }

    [TestMethod]
    public void RecoveryPointFilePath_ShouldIgnoreManuallySetPathsWhenDerived()
    {
        new LogFileCompressionRequestBuilder
        {
            OutputFilePath = "foo",
            ShouldUseRecoveryPoints = true,
            UserDefinedRecoveryPointFilePath = "foo.bar"
        }.RecoveryPointFilePath.ShouldBe($"foo{LogFileCompressionRequestBuilder.DefaultRecoveryPointFileExtension}");
    }

    [TestMethod]
    public void RecoveryOutputFilePath_IsEmptyStringWhenRecoveryPointsAreDisabled()
    {
        new LogFileCompressionRequestBuilder
        {
            OutputFilePath = "foo",
            ShouldUseRecoveryPoints = false
        }.RecoveryPointFilePath.ShouldBe(string.Empty);

        new LogFileCompressionRequestBuilder
        {
            ShouldUseRecoveryPoints = false,
            OutputFilePath = "foo",
            UserDefinedRecoveryPointFilePath = "foo.bar",
        }.RecoveryPointFilePath.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void RecoveryPointFilePath_ShouldCopyManuallySetPathWhenNotDerived()
    {
        new LogFileCompressionRequestBuilder
        {
            OutputFilePath = "foo",
            UserDefinedRecoveryPointFilePath = "foo.bar",
            ShouldDeriveRecoveryPointFilePath = false,
            ShouldUseRecoveryPoints = true,
        }.RecoveryPointFilePath.ShouldBe("foo.bar");
    }

    [TestMethod]
    public void RequestedFileSizeIsAnError_WhenStringIsNotAnInteger()
    {
        new LogFileCompressionRequestBuilder
        {
            RequestedLogFileSizeString = "abc"
        }.RequestedLogFileSize.IsError.ShouldBeTrue();
    }

    [TestMethod]
    public void RequestIsAnError_WhenFileSizeIsAnError()
    {
        new LogFileCompressionRequestBuilder
        {
            RequestedLogFileSizeString = "abc"
        }.Request.IsError.ShouldBeTrue();
    }

    [TestMethod]
    public void DefaultObject_BuildsAValidRequest()
    {
        new LogFileCompressionRequestBuilder().Request.IsSuccess.ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(100, 3, 300)]
    [DataRow(1000, 3, 3000)]
    public void RequestedFileSize_IsScaledByUnitOfMeasure(
        long unitOfMeasure,
        long fileSize,
        long expectedResult
    )
    {
        new LogFileCompressionRequestBuilder
            {
                LogSizeUnitOfMeasure = new DataSize(unitOfMeasure),
                RequestedLogFileSizeString = fileSize.ToString()
            }
            .RequestedLogFileSize
            .Value
            .ByteCount.ShouldBe(expectedResult);
    }
}
