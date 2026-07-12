using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZLibBindings;


public static partial class ZLibLowLevelBindings
{
    [LibraryImport("zlib1.dll")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr zlibVersion();

    public static string ZlibVersion() =>
        Marshal.PtrToStringUTF8(zlibVersion())!;
}


public unsafe struct z_stream_s
{
    byte** next_in;
    ushort avail_in;
    uint total_in;

    byte** next_out;
    ushort avail_out;
    uint total_out;

    byte* msg;

    void** state;


}
