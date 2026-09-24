namespace ArchiveAccessPointVisualizer;

public record DataSize(long ByteCount)
{
    public static DataSize FromMegaBytes(long megaByteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(megaByteCount);

        checked
        {
            return new DataSize(megaByteCount * 1024 * 1024);
        }
    }

    public static DataSize FromGigaBytes(long gigaByteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gigaByteCount);

        checked
        {
            return new DataSize(gigaByteCount * 1024 * 1024 * 1024);
        }
    }

    public static implicit operator long(DataSize size) => size.ByteCount;
}
