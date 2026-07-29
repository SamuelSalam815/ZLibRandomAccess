using CsvHelper.Configuration;
using ZLibWrapper;

namespace LogSimulator.Appliction.AccessPointWriting;

public sealed class RecoveryPointOffsetCsvMap : ClassMap<RecoveryPointOffset>
{
    public RecoveryPointOffsetCsvMap()
    {
        Map(m => m.OffsetInUncompressedStream).Index(0).Name("raw");
        Parameter(nameof(RecoveryPointOffset.OffsetInUncompressedStream)).Index(0).Name("raw");

        Map(m => m.OffsetInCompressedStream).Index(1).Name("compressed");
        Parameter(nameof(RecoveryPointOffset.OffsetInCompressedStream)).Index(1).Name("compressed");
    }
}
