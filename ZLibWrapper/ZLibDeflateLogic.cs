using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper;

internal static unsafe class ZLibDeflateLogic
{
    public static ZLibWriteAction GetNextAction(z_stream_s* zLibStream, ZReturnCode returnCode,
        ZFlushValue  flushValue)
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_OK:
                return HandleOk(zLibStream, flushValue);

            case ZReturnCode.Z_STREAM_END:
                return ZLibWriteAction.CompleteInput;

            case ZReturnCode.Z_BUF_ERROR:
                return HandleBufferError(zLibStream);
            default:
                return ZLibWriteAction.FailToDecide;
        }
    }

    private static ZLibWriteAction HandleOk(z_stream_s* zLibStream, ZFlushValue flushValue)
    {
        if (flushValue == ZFlushValue.Z_NO_FLUSH)
        {
            return ZLibWriteAction.Continue;
        }

        var outputFullyConsumed = zLibStream->avail_out == 0;
        return outputFullyConsumed ? ZLibWriteAction.RequestMoreOutputSpace : ZLibWriteAction.CompleteInput;
    }

    private static ZLibWriteAction HandleBufferError(z_stream_s* zLibStream)
    {
        if (zLibStream->avail_in == 0)
        {
            return ZLibWriteAction.CompleteInput;
        }

        if (zLibStream->avail_out == 0)
        {
            return ZLibWriteAction.RequestMoreOutputSpace;
        }

        return ZLibWriteAction.FailToDecide;
    }
}
