using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio.Exporters;

public class CompressionResultCsvExporter : ICompressionResultExporter, IDisposable
{
    private readonly Stream outputStream;

    public CompressionResultCsvExporter(Stream outputStream)
    {
        // TODO make csv writer and write headers
    }

    public void Accept(CompressionResult result)
    {
        throw new NotImplementedException();
    }

    public void Complete()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        outputStream.Dispose();
    }
}
