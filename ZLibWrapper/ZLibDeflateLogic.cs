using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper;

internal class ZLibDeflateLogic
{
    public static ZLibDeflateAction DecideNextAction(z_stream_s zLibStream, ZReturnCode returnCode)
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_OK:
                return ZLibDeflateAction.CallDeflateAgain;
            case ZReturnCode.Z_BUF_ERROR:
                if (zLibStream.avail_in == 0)
                {
                    return ZLibDeflateAction.InputBufferConsumed;
                }

                if (zLibStream.avail_out == 0)
                {
                    return ZLibDeflateAction.OutputBufferConsumed;
                }

                return ZLibDeflateAction.FailedToDecide;

            case ZReturnCode.Z_STREAM_END:
            case ZReturnCode.Z_NEED_DICT:
                return ZLibDeflateAction.FailedToDecide;

            case ZReturnCode.Z_ERRNO:
            case ZReturnCode.Z_STREAM_ERROR:
            case ZReturnCode.Z_DATA_ERROR:
            case ZReturnCode.Z_MEM_ERROR:
            case ZReturnCode.Z_VERSION_ERROR:
            default:
                return ZLibDeflateAction.FatalError;
        }
    }
}
