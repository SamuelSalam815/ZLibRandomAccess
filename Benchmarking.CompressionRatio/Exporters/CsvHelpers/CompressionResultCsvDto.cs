using Benchmarking.CompressionRatio.Values;
using JetBrains.Annotations;

namespace Benchmarking.CompressionRatio.Exporters.CsvHelpers;

/// <summary>
/// The purpose of this class is to translate from domain type <see cref="CompressionResult"/> to a type most suited
/// for use with <see cref="CsvHelper"/>
/// </summary>
public class CompressionResultCsvDto
{
    public CompressionResultCsvDto(CompressionResult result)
    {
        CompressionMethod = result.Request.CompressionMethod;
        CompressionLevel = result.Request.CompressionLevel;
        NumberOfUncompressedBytes = result.Request.NumberOfBytesToCompress;
        NumberOfCompressedBytes = result.NumberOfBytesAfterCompression;
    }

    [UsedImplicitly(Reason = "Used by CsvHelper")]
    public CompressionResultCsvDto()
    {
    }

    public CompressionMethod CompressionMethod { get; set; }
    public NormalizedCompressionLevel CompressionLevel { get; set; }
    public long NumberOfUncompressedBytes { get; set; }
    public long NumberOfCompressedBytes { get; set; }

    public CompressionResult AsDomainObject()
    {
        return new CompressionResult(
            new CompressionRequest(
                CompressionMethod,
                CompressionLevel,
                NumberOfUncompressedBytes
            ),
            NumberOfCompressedBytes
        );
    }
}
