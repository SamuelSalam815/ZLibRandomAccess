using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.BenchmarkRunners;

public class FakeBenchmarkRunner : ICompressionBenchmarkRunner
{
    public CompressionResult Run(CompressionRequest request, CancellationToken cancellationToken)
    {
        const int baseSleepTime = 1000;
        const double compressionRatio = 0.95d;

        var scaledSleepTime = TimeSpan.FromSeconds(1) * (request.NumberOfBytesToCompress / (1024L * 1024L * 1024L));
        var compressedByteCount = (long)(request.NumberOfBytesToCompress * (1 - compressionRatio));

        Task.Delay(baseSleepTime + (int)scaledSleepTime.TotalMilliseconds, cancellationToken).Wait(cancellationToken);
        return new CompressionResult(request, compressedByteCount);
    }
}
