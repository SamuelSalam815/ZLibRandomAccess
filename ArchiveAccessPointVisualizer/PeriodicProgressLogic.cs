namespace ArchiveAccessPointVisualizer;

public record ProgressReport<T>(T TotalProgress, bool IsFinalReport);

public record PeriodicProgressLogic<T>(
    TimeSpan ProgressCheckInterval,
    Func<ProgressReport<T>> ProgressCheckMethod,
    DateTime CurrentTime,
    T CurrentProgress)
{
    public TimeSpan MinimumDelayBeforeNextReport { get; private init; } = TimeSpan.Zero;
    public bool IsTerminated { get; private init; } = false;

    public PeriodicProgressLogic<T> ProgressIfNeeded(DateTime now)
    {
        // TODO
        throw new NotImplementedException();
    }
}

public class PeriodicProgressRunner<T> where T : notnull
{
    private PeriodicProgressLogic<T> _periodicProgressLogic;

    public PeriodicProgressRunner(PeriodicProgressLogic<T> periodicProgressLogic)
    {
        _periodicProgressLogic = periodicProgressLogic;
    }

    public async Task Run(IProgress<T> progressReporter, CancellationToken cancellationToken = default)
    {
        while (!_periodicProgressLogic.IsTerminated)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(_periodicProgressLogic.MinimumDelayBeforeNextReport, cancellationToken);
            _periodicProgressLogic = _periodicProgressLogic.ProgressIfNeeded(DateTime.Now);
            progressReporter.Report(_periodicProgressLogic.CurrentProgress);
        }
    }
}
