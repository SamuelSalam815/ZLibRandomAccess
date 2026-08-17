using System.Text;
using ZLibWrapper;

namespace ArchiveViewerBackend.TextSearching;

public static class TextSearcherFactory

{
    public static ParallelScanTextSearcher CreateParallelTextSearcher(
        Func<GZipReadingStreamWithRecoveryPoints> gzipStreamFactory,
        List<RecoveryPointOffset> recoveryPoints
    )
    {
        return CreateParallelTextSearcher(gzipStreamFactory, recoveryPoints, Encoding.Default);
    }

    public static ParallelScanTextSearcher CreateParallelTextSearcher(
        Func<GZipReadingStreamWithRecoveryPoints> gzipStreamFactory,
        List<RecoveryPointOffset> recoveryPoints,
        Encoding encoding
    )
    {
        const int byteOverlap = 3 * 1024;
        var streams = new List<WithByteOffset<Stream>>();
        for (var i = 0; i < recoveryPoints.Count; i++)
        {
            var recoveryPoint = recoveryPoints[i];
            var uncompressedOffset = recoveryPoint.OffsetInUncompressedStream;
            var gzipStream = gzipStreamFactory();
            gzipStream.JumpTo(i);
            if (i + 1 < recoveryPoints.Count)
            {
                var segmentLength = recoveryPoints[i + 1].OffsetInUncompressedStream -
                                    uncompressedOffset;
                streams.Add(
                    new StreamWithByteLimit(gzipStream, segmentLength + byteOverlap)
                        .WithByteOffset<Stream>(uncompressedOffset));
            }
            else
            {
                streams.Add(gzipStream.WithByteOffset<Stream>(uncompressedOffset));
            }
        }

        return new ParallelScanTextSearcher(streams, encoding);
    }
}
