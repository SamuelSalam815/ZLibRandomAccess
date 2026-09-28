using Benchmarking.CompressionRatio.Values;

namespace Benchmarking.CompressionRatio;

public class CompressionResultCsvWriter : IDisposable, IAsyncDisposable
{
    private readonly Stream outputStream;

    public CompressionResultCsvWriter(Stream outputStream)
    {
        // TODO make csv writer and write headers
    }

    public void Write(CompressionResult result)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        outputStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await outputStream.DisposeAsync();
    }
}
