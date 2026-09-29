using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.BenchmarkRunners;

internal interface ICompressionBenchmarkRunner
{
    CompressionResult Run(CompressionRequest request);
}
