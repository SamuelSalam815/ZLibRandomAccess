using System.IO.Compression;
using System.Text;
using ArchiveViewerBackend.TextSearching;
using ZLibWrapper;

namespace Benchmarking;

public interface IGzipBenchmarkSetUpStrategy
{
    public Stream CreateCompressingStream(Stream outputStream);

    public ITextSearcher CreateCompressedStreamTextSearcher(Stream compressedStream);
}

public record SystemGzipSetUpStrategy(Encoding Encoding) : IGzipBenchmarkSetUpStrategy
{
    public Stream CreateCompressingStream(Stream outputStream)
    {
        return new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true);
    }

    public ITextSearcher CreateCompressedStreamTextSearcher(Stream compressedStream)
    {
        var decompressor = new GZipStream(compressedStream, CompressionMode.Decompress);
        return new ScanTextSearcher(decompressor, Encoding);
    }
}

public record ZlibGzipSetUpStrategy(Encoding Encoding) : IGzipBenchmarkSetUpStrategy
{
    public Stream CreateCompressingStream(Stream outputStream)
    {
        return new GZipWritingStreamWithRecoveryPoints(outputStream, leaveOpen: true);
    }

    public ITextSearcher CreateCompressedStreamTextSearcher(Stream compressedStream)
    {
        return new ScanTextSearcher(
            new GZipReadingStreamWithRecoveryPoints(compressedStream),
            Encoding
        );
    }
}

public record ZlibGzipParallelSetUpStrategy(
    long RecoveryPointByteInterval,
    long SegmentOverlapInBytes,
    Encoding Encoding) : IGzipBenchmarkSetUpStrategy
{
    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];

    public Stream CreateCompressingStream(Stream outputStream)
    {
        var result = new GZipWritingStreamWithRecoveryPoints(
            outputStream,
            leaveOpen: true,
            RecoveryPointByteInterval
        );
        result.RecoveryPointWritten += _recoveryPointOffsets.Add;
        return result;
    }

    public ITextSearcher CreateCompressedStreamTextSearcher(Stream compressedStream)
    {
        return TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                compressedStream,
                recoveryPointOffsets: _recoveryPointOffsets
            ),
            _recoveryPointOffsets,
            Encoding,
            SegmentOverlapInBytes
        );
    }
}
