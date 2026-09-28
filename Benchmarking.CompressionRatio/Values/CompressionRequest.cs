namespace Benchmarking.CompressionRatio.Values;

public sealed record CompressionRequest(
    CompressionMethod CompressionMethod,
    NormalizedCompressionLevel CompressionLevel,
    long NumberOfBytesToCompress
)
{
    public static IEnumerable<CompressionRequest> AllCombinationsOf(
        IEnumerable<CompressionMethod> methods,
        IEnumerable<NormalizedCompressionLevel> compressionLevels,
        IEnumerable<long> numberOfBytesToCompress)
    {
        throw new NotImplementedException();
    }
}
