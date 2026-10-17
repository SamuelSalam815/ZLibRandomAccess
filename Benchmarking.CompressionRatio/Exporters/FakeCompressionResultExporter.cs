using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.Exporters;

public class FakeCompressionResultExporter(string name) : ICompressionResultExporter
{
    public void Accept(CompressionResult result)
    {
        Console.WriteLine("{0}{1} received {2}...", nameof(FakeCompressionResultExporter), name, result);
    }

    public void Complete()
    {
        Console.WriteLine($"{nameof(FakeCompressionResultExporter)}.{nameof(Complete)}");
    }

    public void Dispose()
    {
        // No resources to release
    }
}
