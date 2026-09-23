using System.Text;

namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionRequest(
    string OutputFilePath,
    RecoveryPointFileDetails? RecoveryPointFileDetails,
    Encoding Encoding,
    DataSize RequestedLogSize
);
