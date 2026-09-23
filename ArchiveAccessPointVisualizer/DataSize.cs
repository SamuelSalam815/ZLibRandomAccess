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
}

public record DataSizeAsUnitString(DataSize DataSizeUnit)
{
    public string DisplayString {
        get
        {
            if (DataSizeUnit == DataSize.FromGigaBytes(1)) return "GB";
            return DataSizeUnit == DataSize.FromMegaBytes(1) ? "MB" : "Custom";
        }
    }
}
