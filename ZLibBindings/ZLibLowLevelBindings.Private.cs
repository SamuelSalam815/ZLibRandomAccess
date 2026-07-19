using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLibBindings.Constants;
using unsafe z_streamp = ZLibBindings.State.z_stream_s*;

namespace ZLibBindings;

public static unsafe partial class ZLibLowLevelBindings
{
    private const string ZlibFileName = "z.dll";

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr zlibVersion();

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ZReturnCode deflateInit_(
        z_streamp strm,
        ZCompressionLevel level,
        IntPtr version,
        int stream_size);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ZReturnCode deflateInit2_(
        z_streamp strm,
        ZCompressionLevel level,
        ZDeflateCompressionMethod method,
        ZWindowBits windowBits,
        ZMemoryLevel memLevel,
        ZCompressionStrategy strategy,
        IntPtr version,
        int stream_size);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ZReturnCode inflateInit_(z_streamp strm, IntPtr version, int stream_size);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ZReturnCode inflateInit2_(
        z_streamp strm,
        ZWindowBits windowBits,
        IntPtr version,
        int stream_size);
}
