using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLibBindings.Constants;
using ZLibBindings.State;
using unsafe z_streamp = ZLibBindings.State.z_stream_s*;

namespace ZLibBindings;

public static unsafe partial class ZLibLowLevelBindings
{
    public static string GetZlibVersion() =>
        Marshal.PtrToStringUTF8(zlibVersion())!;

    public static ZReturnCode deflateInit(z_streamp strm, ZCompressionLevel level = ZCompressionLevel.Z_DEFAULT_COMPRESSION) => deflateInit_(strm, level, zlibVersion(), sizeof(z_stream_s));

    public static ZReturnCode deflateInit2(
        z_streamp strm,
        ZCompressionLevel level = ZCompressionLevel.Z_DEFAULT_COMPRESSION,
        ZDeflateCompressionMethod method = ZDeflateCompressionMethod.Z_DEFLATED,
        ZWindowBits windowBits = ZWindowBits.Default,
        ZMemoryLevel memLevel = ZMemoryLevel.Default,
        ZCompressionStrategy strategy = ZCompressionStrategy.Z_DEFAULT_STRATEGY
    ) => deflateInit2_(strm, level, method, windowBits, memLevel, strategy, zlibVersion(), sizeof(z_stream_s));

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflate(z_streamp strm, ZFlushValue flush);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflatePending(z_streamp strm, uint* pending, int* bits);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode deflateEnd(z_streamp strm);

    public static ZReturnCode inflateInit(z_streamp strm) => inflateInit_(strm, zlibVersion(), sizeof(z_stream_s));

    public static ZReturnCode inflateInit2(z_streamp strm, ZWindowBits windowBits = ZWindowBits.Default) =>
        inflateInit2_(strm, windowBits, zlibVersion(), sizeof(z_stream_s));


    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode inflate(z_streamp strm, ZFlushValue flush);

    [LibraryImport(ZlibFileName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ZReturnCode inflateEnd(z_streamp strm);
}
