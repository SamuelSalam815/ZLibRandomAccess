using ZLibBindings.Constants;

namespace ZLibWrapper.Logic;

internal record RecoveryPointNavigationLogic(bool DoesHeaderNeedToBeRead)
{
    public static RecoveryPointNavigationLogic CreateFrom(ZWindowBits windowBits)
    {
        var isRawDeflateStream = (int)windowBits < 0;
        return new RecoveryPointNavigationLogic(!isRawDeflateStream);
    }

    public RecoveryPointNavigationLogic GetJumpAction(out RecoveryPointNavigationAction action)
    {
        if (DoesHeaderNeedToBeRead)
        {
            action = RecoveryPointNavigationAction.ReadHeader;
            return new RecoveryPointNavigationLogic(DoesHeaderNeedToBeRead: false);
        }

        action = RecoveryPointNavigationAction.JumpToRecoveryPoint;
        return this;
    }
}
