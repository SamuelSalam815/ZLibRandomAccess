using ErrorOr;

namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionJobStatus(
    bool IsRunning,
    long NumberOfBytesWritten,
    long TargetNumberOfBytes,
    string JobStatusDescription
)
{
    public static readonly LogFileCompressionJobStatus NotReady = new(
        false,
        0,
        100,
        string.Empty
    );

    public LogFileCompressionJobStatus Accept(ErrorOr<LogFileCompressionProgressReport> reportOrError)
    {
        return reportOrError
            .Match(
                report =>
                    this with
                    {
                        IsRunning = !report.IsJobComplete,
                        NumberOfBytesWritten = report.TotalNumberOfBytesWritten,
                        JobStatusDescription = report.StatusDescription ?? JobStatusDescription
                    },
                error =>
                    this with
                    {
                        IsRunning = false,
                        JobStatusDescription = string.Join(Environment.NewLine, error.Select(e => e.Description))
                    }
            );
    }
}
