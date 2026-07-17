using ZLibBindings.Constants;
using ZLibBindings.State;

namespace ZLibWrapper.Extensions;

public static unsafe class ZReturnCodeExtensions
{
    public static ZReturnCode GuardAgainstFatalErrors(
        this ZReturnCode returnCode,
        z_stream_s* streamState
    )
    {
        switch (returnCode)
        {
            case ZReturnCode.Z_ERRNO:
            case ZReturnCode.Z_STREAM_ERROR:
            case ZReturnCode.Z_DATA_ERROR:
            case ZReturnCode.Z_MEM_ERROR:
            case ZReturnCode.Z_VERSION_ERROR:
                var errorMessage = (*streamState).GetErrorMessage();
                throw new ZLibException(errorMessage, returnCode);
            default:
                return returnCode;
        }
    }
}
