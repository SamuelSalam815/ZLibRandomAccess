using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.Exporters;

public interface ICompressionResultExporter
{
    void Accept(CompressionResult result);
    void Complete();
}
