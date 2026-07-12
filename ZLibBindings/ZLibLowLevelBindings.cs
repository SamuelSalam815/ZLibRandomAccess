using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZLibBindings;

public static partial class ZLibLowLevelBindings
{
    [LibraryImport("zlib1.dll", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial string zlibVersion();
}
