namespace ArchiveViewerBackend;

public record WithByteOffset<T>(T Payload, long ByteOffset);

public static class WithByteOffsetExtensions
{
    public static WithByteOffset<T> WithByteOffset<T>(this T payload, long byteOffset)
    {
        return new WithByteOffset<T>(payload, byteOffset);
    }
}
