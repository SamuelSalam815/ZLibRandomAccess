using System.IO.Compression;

namespace Benchmarking.CompressionRatio;

public sealed record SystemGzipCompressionRequest(CompressionLevel CompressionLevel) : CompressionRequest
{
    protected override Stream CreateCompressedStream(Stream outputStream)
    {
        return new GZipStream(outputStream, CompressionLevel, leaveOpen: true);
    }
}