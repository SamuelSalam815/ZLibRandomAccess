using JetBrains.Annotations;

namespace ArchiveViewerBackend;

/// <param name="totalNumberOfBytes">The number of bytes in the stream to split.</param>
/// <param name="byteOverlap">How many bytes should overlap between splits</param>
public class StreamSplitterLogic(long totalNumberOfBytes, long byteOverlap)
{
    public record SplitSpec(long ByteOffset, long ByteLength);

    [Pure]
    public IEnumerable<SplitSpec> DescribeSplits(List<long> offsetsToSplitOn)
    {
        if (offsetsToSplitOn.Count == 0)
        {
            yield return new SplitSpec(0, totalNumberOfBytes);

            yield break;
        }

        yield return new SplitSpec(0, Math.Min(offsetsToSplitOn[0] + byteOverlap, totalNumberOfBytes));

        for (var i = 0; i < offsetsToSplitOn.Count - 1; i++)
        {
            var currentOffset = offsetsToSplitOn[i];
            var nextOffset = offsetsToSplitOn[i + 1];
            var length = nextOffset - currentOffset + byteOverlap;
            length = Math.Min(length, totalNumberOfBytes - currentOffset);
            yield return new SplitSpec(currentOffset, length);
        }

        yield return new SplitSpec(offsetsToSplitOn[^1], totalNumberOfBytes - offsetsToSplitOn[^1]);
    }
}
