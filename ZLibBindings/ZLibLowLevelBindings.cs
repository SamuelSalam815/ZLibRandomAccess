using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLibBindings.Constants;
using unsafe z_streamp = ZLibBindings.State.z_stream_s*;

namespace ZLibBindings;

public static unsafe partial class ZLibLowLevelBindings
{
    private const string ZlibLibrary = "z.dll";

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr zlibVersion();

    public static string ZlibVersion() =>
        Marshal.PtrToStringUTF8(zlibVersion())!;

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflateInit(z_streamp strm, ZCompressionLevel level);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflate(z_streamp strm, ZFlushValue flush);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflatePending(z_streamp strm, uint* pending, int* bits);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflateEnd(z_streamp strm);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode inflateInit(z_streamp strm);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode inflate(z_streamp strm, ZFlushValue flush);

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode inflateEnd(z_streamp strm);
}
