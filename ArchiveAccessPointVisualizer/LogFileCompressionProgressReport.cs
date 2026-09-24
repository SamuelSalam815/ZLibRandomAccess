namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionProgressReport(
    long TotalNumberOfBytesWritten,
    string? StatusDescription,
    bool IsJobComplete = false
);
