namespace ZLibBindings.Constants;

[Flags]
public enum ZWindowBits
{
    #region Raw Deflate
    // When a raw deflate stream is used no header is written.
    // For the other kinds of stream, the header is where the window size is embedded.
    // Since raw deflate streams have no header, the application must provide the same window size when inflating
    // (decompressing) that stream

    /// <summary>
    /// Uses a window size of 2^15 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize32Kb = -15,

    /// <summary>
    /// Uses a window size of 2^14 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize16Kb,

    /// <summary>
    /// Uses a window size of 2^13 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize8Kb,

    /// <summary>
    /// Uses a window size of 2^12 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize4Kb,

    /// <summary>
    /// Uses a window size of 2^11 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize2Kb,

    /// <summary>
    /// Uses a window size of 2^10 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize1Kb,

    /// <summary>
    /// Uses a window size of 2^9 bytes. No header is written.
    /// </summary>
    RawDeflateStreamWindowSize512B,
    #endregion

    #region Window Sizes with Headers
    /// <summary>
    /// Uses a window size of 2^9 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize512B = 9,

    /// <summary>
    /// Uses a window size of 2^10 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize1Kb,

    /// <summary>
    /// Uses a window size of 2^11 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize2Kb,

    /// <summary>
    /// Uses a window size of 2^12 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize4Kb,

    /// <summary>
    /// Uses a window size of 2^13 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize8Kb,

    /// <summary>
    /// Uses a window size of 2^14 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize16Kb,

    /// <summary>
    /// Uses a window size of 2^15 bytes. The use of ZLib header is expected.
    /// </summary>
    WindowSize32Kb,
    #endregion

    /// <summary>
    /// This flag makes the use of a GZip header to be expected instead of a ZLib header.
    /// </summary>
    GZipStream = 16,

    /// <summary>
    /// This flag should only be used when inflating (decompressing). It indicates a ZLib or GZip header is expected,
    /// and the type of header should be determined automatically.
    /// </summary>
    AutoDetectHeader = 32,

    /// <summary>
    /// The default window size - 2^15 bytes and the use of ZLib header being expected.
    /// </summary>
    DefaultWindowSize = WindowSize32Kb,
}
