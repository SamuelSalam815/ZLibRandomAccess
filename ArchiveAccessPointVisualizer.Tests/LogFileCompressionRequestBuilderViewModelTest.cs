using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(LogFileCompressionRequestBuilderViewModel))]
public class LogFileCompressionRequestBuilderViewModelTest
{
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
        new LogFileCompressionRequestBuilderViewModel
        {
            ShouldUseRecoveryPoints = isRecoveryPointEnabled,
            ShouldDeriveRecoveryPointFilePath = isRecoveryPointFilePathDerived
        }.CanUserSpecifyRecoveryPointFilePath.ShouldBe(expectedResult);
    }
}
