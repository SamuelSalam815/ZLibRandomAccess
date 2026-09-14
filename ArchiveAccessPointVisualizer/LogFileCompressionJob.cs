using System.IO;
using System.Text;

namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionJob
{
    public LogFileCompressionJob(string OutputFilePath, long RequestedUncompressedLogSizeInBytes, Encoding Encoding, long? RecoveryPointByteInterval = null)
    {
        if (Path.GetExtension(OutputFilePath) != ".gz")
        {
            throw new ArgumentException("Output file path is not to a gz file!", nameof(OutputFilePath));
        }

        this.OutputFilePath = OutputFilePath;
        this.RequestedUncompressedLogSizeInBytes = RequestedUncompressedLogSizeInBytes;
        this.Encoding = Encoding;
        this.RecoveryPointByteInterval = RecoveryPointByteInterval;

        if (RecoveryPointByteInterval is not null)
        {
            RecoveryPointFilePath = Path.ChangeExtension(OutputFilePath, "csv");
        }
    }

    public string OutputFilePath { get; }
    public string? RecoveryPointFilePath { get; }
    public long RequestedUncompressedLogSizeInBytes { get; }
    public Encoding Encoding { get; }
    public long? RecoveryPointByteInterval { get; }
}
