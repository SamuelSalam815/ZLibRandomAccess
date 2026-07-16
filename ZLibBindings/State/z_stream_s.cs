using ZLibBindings.Constants;
using unsafe Bytef = byte*;
using unsafe Voidpf = void**;

namespace ZLibBindings.State;

public unsafe struct z_stream_s
{
    /// <summary>
    /// Next input byte
    /// </summary>
    Bytef* next_in;

    /// <summary>
    /// Number of bytes available at next_in
    /// </summary>
    uint avail_in;

    /// <summary>
    /// Total number of input bytes read so far
    /// </summary>
    uint total_in;

    /// <summary>
    /// The next output byte will go here
    /// </summary>
    Bytef* next_out;

    /// <summary>
    /// The remaining free space at next_out
    /// </summary>
    uint avail_out;

    /// <summary>
    /// Total number of bytes output so far
    /// </summary>
    ulong total_out;

    /// <summary>
    /// Last error message, NULL if no error
    /// </summary>
    byte* msg;

    /// <summary>
    /// Internal state for the library
    /// </summary>
    void** state;

    /// <summary>
    /// Used to allocate the internal state
    /// </summary>
    IntPtr zalloc;

    /// <summary>
    /// Used to free the internal state
    /// </summary>
    IntPtr zfree;

    /// <summary>
    /// Application-defined pointer that is passed to zalloc and zfree
    /// </summary>
    Voidpf opaque;

    /// <summary>
    /// best guess about the data type: binary or text
    /// for deflate, or the decoding state for inflate
    /// </summary>
    ZDataType _zDataType;

    /// <summary>
    /// Adler-32 or CRC-32 value of the uncompressed data
    /// </summary>
    ulong adler;

    /// <summary>
    /// Reserved for future use
    /// </summary>
    ulong reserved;
}
