using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper.Logic;

internal static unsafe class ZLibPumpLogic
{
    public static ZLibPumpAction GetNextAction(z_stream_s* zLibStream, ZReturnCode returnCode,
        ZFlushValue  flushValue)
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_OK:
                return HandleOk(zLibStream, flushValue);

            case ZReturnCode.Z_STREAM_END:
                return ZLibPumpAction.RequestMoreInputSpace;

            case ZReturnCode.Z_BUF_ERROR:
                return HandleBufferError(zLibStream);
            default:
                return ZLibPumpAction.FailToDecide;
        }
    }

    private static ZLibPumpAction HandleOk(z_stream_s* zLibStream, ZFlushValue flushValue)
    {
        if (flushValue == ZFlushValue.Z_NO_FLUSH)
        {
            return ZLibPumpAction.Continue;
        }

        var outputFullyConsumed = zLibStream->avail_out == 0;
        return outputFullyConsumed ? ZLibPumpAction.RequestMoreOutputSpace : ZLibPumpAction.RequestMoreInputSpace;
    }

    private static ZLibPumpAction HandleBufferError(z_stream_s* zLibStream)
    {
        if (zLibStream->avail_in == 0)
        {
            return ZLibPumpAction.RequestMoreInputSpace;
        }

        if (zLibStream->avail_out == 0)
        {
            return ZLibPumpAction.RequestMoreOutputSpace;
        }

        return ZLibPumpAction.FailToDecide;
    }
}
