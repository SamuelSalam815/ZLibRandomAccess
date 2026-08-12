namespace ArchiveViewerBackend;

public record WithByteOffset<T>(T Payload, int ByteOffset);

public static class WithByteOffsetExtensions
{
    public static WithByteOffset<T> WithByteOffset<T>(this T payload, int byteOffset)
    {
        return new WithByteOffset<T>(payload, byteOffset);
    }
}
