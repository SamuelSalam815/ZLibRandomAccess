namespace Benchmarking.CompressionRatio.Values;

/// <summary>
/// Specifies a format-independent level of compression to use
/// </summary>
public enum NormalizedCompressionLevel
{
    /// <summary>
    /// Use the compression level chosen by the library as the best trade off.
    /// </summary>
    Default,

    /// <summary>
    /// Use the level that produces the smallest result.
    /// This is expected to yield the best compression ratio for a given compression method.
    /// </summary>
    Smallest,

    /// <summary>
    /// Use the level that produces the compressed result fastest.
    /// This is expected to yield the worst compression ratio for a given compression method.
    /// </summary>
    Fastest,
}
