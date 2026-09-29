using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.Exporters;

public interface ICompressionResultExporter : IDisposable
{
    void Accept(CompressionResult result);
    void Complete();
}
