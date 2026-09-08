using System.Globalization;
using System.Text;
using CsvHelper;

namespace ZLibWrapper;

public class RecoveryPointOffsetCsvWriter : IDisposable, IAsyncDisposable
{
    private readonly CsvWriter _csvWriter;

    public RecoveryPointOffsetCsvWriter(Stream outputStream, Encoding encoding, bool leaveOpen = false)
    {
        var streamWriter = new StreamWriter(outputStream, encoding, leaveOpen: leaveOpen);
        _csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        _csvWriter.WriteHeader<RecoveryPointOffset>();
        _csvWriter.NextRecord();
    }

    public void Dispose()
    {
        _csvWriter.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _csvWriter.DisposeAsync();
    }

    public void WriteAll(IEnumerable<RecoveryPointOffset> records)
    {
        foreach (var record in records)
        {
            Write(record);
        }
    }

    public void Write(RecoveryPointOffset record)
    {
        _csvWriter.WriteRecord(record);
        _csvWriter.NextRecord();
    }
}
