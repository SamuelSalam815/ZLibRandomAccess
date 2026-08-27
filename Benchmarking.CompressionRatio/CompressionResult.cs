namespace Benchmarking.CompressionRatio;

public record CompressionResult(CompressionRequest CompressionRequest, long InputDataSize, long OutputDataSize)
{
    public double CompressionRatio => OutputDataSize / (double)InputDataSize;
}