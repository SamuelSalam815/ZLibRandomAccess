using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;
using ZLibWrapper.Logic;

namespace ZLibWrapper.Tests.Logic;

[TestClass]
[TestSubject(typeof(RecoveryPointNavigationLogic))]
public class RecoveryPointNavigationLogicTest
{
    [TestMethod]
    [DataRow(ZWindowBits.WindowSize512B, true)]
    [DataRow(ZWindowBits.WindowSize32Kb, true)]
    [DataRow(ZWindowBits.WindowSize512B | ZWindowBits.GZipStream, true)]
    [DataRow(ZWindowBits.WindowSize32Kb | ZWindowBits.GZipStream, true)]
    [DataRow(ZWindowBits.WindowSize512B | ZWindowBits.AutoDetectHeader, true)]
    [DataRow(ZWindowBits.WindowSize32Kb | ZWindowBits.AutoDetectHeader, true)]
    [DataRow(ZWindowBits.RawDeflateStreamWindowSize512B, false)]
    [DataRow(ZWindowBits.RawDeflateStreamWindowSize32Kb, false)]
    public void HeaderNeedsToBeRead_OnlyWhenWindowBitsRequireIt(ZWindowBits windowBits, bool shouldReadHeader)
    {
        RecoveryPointNavigationLogic.CreateFrom(windowBits)
            .DoesHeaderNeedToBeRead
            .ShouldBe(shouldReadHeader);
    }

    [TestMethod]
    public void ReadHeader_WhenHeaderNeedsToBeRead()
    {
        new RecoveryPointNavigationLogic(true)
            .GetJumpAction(out var jumpAction)
            .DoesHeaderNeedToBeRead.ShouldBeFalse();

        jumpAction.ShouldBe(RecoveryPointNavigationAction.ReadHeader);
    }

    [TestMethod]
    public void JumpToRecoverPointAction_WhenHeaderDoesNotNeedToBeRead()
    {
        new RecoveryPointNavigationLogic(false)
            .GetJumpAction(out var jumpAction)
            .DoesHeaderNeedToBeRead.ShouldBeFalse();

        jumpAction.ShouldBe(RecoveryPointNavigationAction.JumpToRecoveryPoint);
    }
}
