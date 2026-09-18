using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(LogGenerationFormValues))]
public class LogGenerationFormValuesTest
{
    [TestMethod]
    [DataRow(@"C:/temp/output.gz", @$"C:/temp/output{ILogGenerationProperties.DefaultRecoveryPointFileExtension}")]
    [DataRow(@"C:/temp/output", @$"C:/temp/output{ILogGenerationProperties.DefaultRecoveryPointFileExtension}")]
    [DataRow(@"foo", @$"foo{ILogGenerationProperties.DefaultRecoveryPointFileExtension}")]
    public void SettingOutputFilePath_DerivesRecoveryPointFilePathAsExpected(
        string newOutputFilePath,
        string expectedDerivedPath)
    {
        new LogGenerationFormValues
        {
            OutputFilePath = newOutputFilePath
        }.DerivedRecoveryPointFilePath.ShouldBe(expectedDerivedPath);
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, true)]
    [DataRow(true, true, false)]
    public void CanSpecifyRecoveryPointFilePath_IfAndOnlyIfRecoveryPointsAreEnabledAndFilePathIsNotDerived(
        bool isRecoveryPointEnabled,
        bool isRecoveryPointFilePathDerived,
        bool expectedResult)
    {
        new LogGenerationFormValues
        {
            ShouldUseRecoveryPoints = isRecoveryPointEnabled,
            ShouldDeriveRecoveryPointFilePath = isRecoveryPointFilePathDerived
        }.CanUserSpecifyRecoveryPointFilePath.ShouldBe(expectedResult);
    }

    [TestMethod]
    public void RecoveryPointFilePath_ShouldIgnoreManuallySetPathsWhenDerived()
    {
        new LogGenerationFormValues
        {
            OutputFilePath = "foo",
            ShouldUseRecoveryPoints = true,
            UserDefinedRecoveryPointFilePath = "foo.bar"
        }.RecoveryPointFilePath.ShouldBe($"foo{ILogGenerationProperties.DefaultRecoveryPointFileExtension}");
    }

    [TestMethod]
    public void RecoveryOutputFilePath_IsEmptyStringWhenRecoveryPointsAreDisabled()
    {
        new LogGenerationFormValues()
        {
            OutputFilePath = "foo",
            ShouldUseRecoveryPoints = false
        }.RecoveryPointFilePath.ShouldBe(string.Empty);

        new LogGenerationFormValues()
        {
            ShouldUseRecoveryPoints = false,
            OutputFilePath = "foo",
            UserDefinedRecoveryPointFilePath = "foo.bar",
        }.RecoveryPointFilePath.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void RecoveryPointFilePath_ShouldCopyManuallySetPathWhenNotDerived()
    {
        new LogGenerationFormValues()
        {
            OutputFilePath = "foo",
            UserDefinedRecoveryPointFilePath = "foo.bar",
            ShouldDeriveRecoveryPointFilePath = false,
            ShouldUseRecoveryPoints = true,
        }.RecoveryPointFilePath.ShouldBe("foo.bar");
    }
}
