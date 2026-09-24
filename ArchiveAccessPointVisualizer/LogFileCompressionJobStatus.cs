using ErrorOr;

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

    public LogFileCompressionJobStatus Accept(ErrorOr<LogFileCompressionProgressReport> reportOrError)
    {
        return reportOrError
            .Match(
                report =>
                    this with
                    {
                        IsRunning = !report.IsJobComplete,
                        NumberOfBytesWritten = report.TotalNumberOfBytesWritten,
                        Description = report.StatusDescription ?? Description
                    },
                error =>
                    this with
                    {
                        IsRunning = false,
                        Description = string.Join(Environment.NewLine, error.Select(e => e.Description))
                    }
            );
    }
}
