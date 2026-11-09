using System.Globalization;
using Benchmarking.CompressionRatio.Exporters.CsvHelpers;
using Benchmarking.CompressionRatio.Values;
using CsvHelper;

namespace Benchmarking.CompressionRatio.Exporters;

public sealed class CompressionResultCsvExporter : ICompressionResultExporter
{
    private readonly CsvWriter _csvWriter;

    public CompressionResultCsvExporter(Stream outputStream)
    {
        _csvWriter = new CsvWriter(new StreamWriter(outputStream), CultureInfo.InvariantCulture);
        _csvWriter.WriteHeader<CompressionResultCsvDto>();
    }

    public void Accept(CompressionResult result)
    {
        _csvWriter.NextRecord();
        _csvWriter.WriteRecord(new CompressionResultCsvDto(result));
    }

    public void Complete()
    {
        Dispose();
    }

    public void Dispose()
    {
        _csvWriter.Dispose();
    }
}
