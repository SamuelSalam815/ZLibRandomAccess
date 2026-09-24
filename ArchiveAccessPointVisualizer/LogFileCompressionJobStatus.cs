namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionJobStatus(
    bool IsRunning,
    long NumberOfBytesWritten,
    long TargetNumberOfBytes,
    string Description
)
{
    public static readonly LogFileCompressionJobStatus NotReady = new(
        false,
        0,
        100,
        string.Empty
    );

    public LogFileCompressionJobStatus Accept(LogFileCompressionProgressReport report)
    {
        throw new NotImplementedException();
    }
}
