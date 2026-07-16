using System.Runtime.InteropServices;
using ZLibBindings.State;

namespace ZLibWrapper.Extensions;

internal static unsafe class ZLibStreamExtensions
{
    public static string? GetErrorMessage(this z_stream_s state)
    {
        return Marshal.PtrToStringUTF8((nint)state.msg);
    }
}
