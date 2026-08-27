using System.Text;

namespace Benchmarking.CompressionRatio;

public abstract record CompressionRequest
{
    public CompressionResult CalculateCompressedPayloadSize(long inputDataSizeInBytes)
    {
        using var compressedData = new MemoryStream();
        using (var compressor = CreateCompressedStream(compressedData))
        {
            var simulator = new LogSimulator.Simulator.LogSimulator();
            simulator.SimulateLogs(compressor, Encoding.Default, inputDataSizeInBytes);
        }

        var outputDataSize = compressedData.Length;
        return new CompressionResult(this, inputDataSizeInBytes, outputDataSize);
    }

    protected abstract Stream CreateCompressedStream(Stream outputStream);
};