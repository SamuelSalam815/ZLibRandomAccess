using System.Diagnostics.CodeAnalysis;
using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper.Logic;

internal static unsafe class ZLibPumpLogic
{
    public static ZLibPumpAction GetNextAction(z_stream_s* zLibStream, ZReturnCode returnCode)
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_OK:
                return ZLibPumpAction.Continue;

            case ZReturnCode.Z_STREAM_END:
                return ZLibPumpAction.CompleteStream;

            case ZReturnCode.Z_BUF_ERROR:
                return TryGetActionForMoreBufferSpace(zLibStream, out var bufferSpaceAction)
                    ? bufferSpaceAction.Value
                    : ZLibPumpAction.FailToDecide;

            default:
                return ZLibPumpAction.FailToDecide;
        }
    }

    private static bool TryGetActionForMoreBufferSpace(z_stream_s* zLibStream, [NotNullWhen(true)]out ZLibPumpAction? action)
    {
        var needMoreInput = zLibStream->avail_in == 0;
        var needMoreOutput = zLibStream->avail_out == 0;

        if (needMoreInput && needMoreOutput)
        {
            action = ZLibPumpAction.RequestMoreOutputSpace;
            return true;
        }

        if (needMoreInput)
        {
            action = ZLibPumpAction.RequestMoreInputSpace;
            return true;
        }

        if (needMoreOutput)
        {
            action = ZLibPumpAction.RequestMoreOutputSpace;
            return true;
        }

        action = null;
        return false;
    }
}
