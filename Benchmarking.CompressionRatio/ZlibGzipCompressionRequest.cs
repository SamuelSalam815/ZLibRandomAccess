using ZLibBindings.Constants;
using ZLibWrapper;

namespace Benchmarking.CompressionRatio;

public sealed record ZlibGzipCompressionRequest(ZCompressionLevel CompressionLevel) : CompressionRequest
{
    protected override Stream CreateCompressedStream(Stream outputStream)
    {
        return new GZipWritingStreamWithRecoveryPoints(
            outputStream,
            leaveOpen: true,
            recoveryPointByteInterval: null,
            CompressionLevel);
    }
}