namespace Benchmarking.CompressionRatio.Values;

/// <summary>
/// Specifies which method to use for compression
/// </summary>
public enum CompressionMethod
{
    /// <summary>
    /// Compress into gzip format using the ZLib library.
    /// </summary>
    GZipUsingZLib,

    /// <summary>
    /// Like <see cref="GZipUsingZLib"/> but adds access points that are used for random access.
    /// </summary>
    GZipUsingZLibWithFullFlushing,

    /// <summary>
    /// Use <see cref="System.IO.Compression.GZipStream"/>.
    /// </summary>
    GZipUsingBaseClassLibrary
}
