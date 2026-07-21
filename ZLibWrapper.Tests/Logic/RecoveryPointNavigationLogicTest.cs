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
    [DataRow(ZWindowBits.ZLib512BWindow, true)]
    [DataRow(ZWindowBits.ZLib32KbWindow, true)]
    [DataRow(ZWindowBits.GZip512BWindow, true)]
    [DataRow(ZWindowBits.GZip32KbWindow, true)]
    [DataRow(ZWindowBits.RawDeflate512BWindow, false)]
    [DataRow(ZWindowBits.RawDeflate32KbWindow, false)]
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
