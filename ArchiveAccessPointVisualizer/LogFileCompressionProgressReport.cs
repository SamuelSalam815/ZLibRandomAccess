namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionProgressReport(
    long NumberOfBytesWritten,
    long TargetNumberOfBytes,
    string? Status
);
