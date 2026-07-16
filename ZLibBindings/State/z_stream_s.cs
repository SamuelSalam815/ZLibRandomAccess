using ZLibBindings.Constants;

namespace ZLibBindings.State;

public unsafe struct z_stream_s
{
    /// <summary>
    /// Next input byte
    /// </summary>
    public byte* next_in;

    /// <summary>
    /// Number of bytes available at next_in
    /// </summary>
    public uint avail_in;

    /// <summary>
    /// Total number of input bytes read so far
    /// </summary>
    public uint total_in;

    /// <summary>
    /// The next output byte will go here
    /// </summary>
    public byte* next_out;

    /// <summary>
    /// The remaining free space at next_out
    /// </summary>
    public uint avail_out;

    /// <summary>
    /// Total number of bytes output so far
    /// </summary>
    public ulong total_out;

    /// <summary>
    /// Last error message, NULL if no error
    /// </summary>
    public byte* msg;

    /// <summary>
    /// Internal state for the library
    /// </summary>
    public void** state;

    /// <summary>
    /// Used to allocate the internal state
    /// </summary>
    public void* zalloc;

    /// <summary>
    /// Used to free the internal state
    /// </summary>
    public void* zfree;

    /// <summary>
    /// Application-defined pointer that is passed to zalloc and zfree
    /// </summary>
    public void* opaque;

    /// <summary>
    /// best guess about the data type: binary or text
    /// for deflate, or the decoding state for inflate
    /// </summary>
    public ZDataType _zDataType;

    /// <summary>
    /// Adler-32 or CRC-32 value of the uncompressed data
    /// </summary>
    public ulong adler;

    /// <summary>
    /// Reserved for future use
    /// </summary>
    public ulong reserved;
}
