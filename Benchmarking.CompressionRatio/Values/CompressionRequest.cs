namespace Benchmarking.CompressionRatio.Values;

public sealed record CompressionRequest(
    CompressionMethod CompressionMethod,
    NormalizedCompressionLevel CompressionLevel,
    long NumberOfBytesToCompress
)
{
    public static IEnumerable<CompressionRequest> AllCombinationsOf(
        List<CompressionMethod> methods,
        List<NormalizedCompressionLevel> compressionLevels,
        List<long> numberOfBytesToCompress)
    {
        return from compressionMethod in methods
            from compressionLevel in compressionLevels
            from size in numberOfBytesToCompress
            select new CompressionRequest(compressionMethod, compressionLevel, size);
    }
}
