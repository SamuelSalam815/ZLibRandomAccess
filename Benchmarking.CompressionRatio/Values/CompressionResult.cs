namespace Benchmarking.CompressionRatio.Values;

public sealed record CompressionResult(
    CompressionRequest Request,
    long NumberOfBytesAfterCompression);