namespace ArchiveAccessPointVisualizer;

public record DataSizeAsUnitString(DataSize DataSizeUnit)
{
    public string DisplayString {
        get
        {
            if (DataSizeUnit == DataSize.FromGigaBytes(1)) return "GB";
            return DataSizeUnit == DataSize.FromMegaBytes(1) ? "MB" : "Custom";
        }
    }

    public override string ToString() => DisplayString;
}