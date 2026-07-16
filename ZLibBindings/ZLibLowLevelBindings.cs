using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZLibBindings;

using unsafe voidpf = void**;
using unsafe Bytef = byte*;

public static partial class ZLibLowLevelBindings
{
    private const string ZlibLibrary = "z.dll";

    [LibraryImport(ZlibLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr zlibVersion();

    public static string ZlibVersion() =>
        Marshal.PtrToStringUTF8(zlibVersion())!;
}


public unsafe delegate voidpf alloc_func(voidpf opaque, uint items, uint size);

public unsafe delegate void free_func(voidpf opaque, voidpf address);

public unsafe struct z_stream_s
{
    Bytef *next_in;
    uint avail_in;
    uint total_in;

    Bytef *next_out;
    uint avail_out;
    ulong total_out;

    byte* msg;
    void** state;

    IntPtr zalloc;
    IntPtr zfree;

}
