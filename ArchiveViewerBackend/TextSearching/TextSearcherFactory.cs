using System.Text;
using ZLibWrapper;

namespace ArchiveViewerBackend.TextSearching;

public static class TextSearcherFactory

{
    public static ParallelScanTextSearcher CreateParallelTextSearcher(
        Func<GZipReadingStreamWithRecoveryPoints> gzipStreamFactory,
        List<RecoveryPointOffset> recoveryPoints,
        Encoding encoding,
        long parallelStreamOverlapInBytes)
    {
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
                    new StreamWithByteLimit(gzipStream, segmentLength + parallelStreamOverlapInBytes)
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
