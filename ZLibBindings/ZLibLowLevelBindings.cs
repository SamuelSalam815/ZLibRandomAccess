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
