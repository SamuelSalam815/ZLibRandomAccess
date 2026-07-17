using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper;

internal static unsafe class ZLibDeflateLogic
{
    public static ZLibWriteAction GetNextAction(z_stream_s* zLibStream, ZReturnCode returnCode)
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_OK:
                return ZLibWriteAction.Continue;

            case ZReturnCode.Z_STREAM_END:
                return ZLibWriteAction.CompleteInput;

            case ZReturnCode.Z_BUF_ERROR:
                if (zLibStream->avail_in == 0)
                {
                    return ZLibWriteAction.CompleteInput;
                }

                if (zLibStream->avail_out == 0)
                {
                    return ZLibWriteAction.RequestMoreOutputSpace;
                }

                return ZLibWriteAction.FailToDecide;
            default:
                return ZLibWriteAction.FailToDecide;
        }
    }
}
