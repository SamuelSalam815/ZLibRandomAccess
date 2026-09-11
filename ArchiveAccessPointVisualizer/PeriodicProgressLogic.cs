namespace ArchiveAccessPointVisualizer;

public record ProgressReport<T>(T TotalProgress, bool IsFinalReport = false)
{
    public static implicit operator ProgressReport<T>(T totalProgress)
    {
        return new ProgressReport<T>(totalProgress);
    }
}

public record PeriodicProgressLogic<T>(
    TimeSpan ProgressCheckInterval,
    Func<ProgressReport<T>> ProgressCheckMethod,
    DateTime CurrentTime,
    T CurrentProgress)
{
    public TimeSpan MinimumDelayBeforeNextReport { get; private init; } = TimeSpan.Zero;
    public bool IsTerminated { get; private init; } = false;

    public PeriodicProgressLogic<T> Update(DateTime now)
    {
        if (IsTerminated)
        {
            return this;
        }

        var timePassed = now - CurrentTime;
        if (timePassed < MinimumDelayBeforeNextReport)
        {
            return this with { MinimumDelayBeforeNextReport = MinimumDelayBeforeNextReport - timePassed };
        }

        var progressReport = ProgressCheckMethod();

        return this with
        {
            CurrentTime = now,
            CurrentProgress = progressReport.TotalProgress,
            IsTerminated = progressReport.IsFinalReport,
            MinimumDelayBeforeNextReport =  ProgressCheckInterval
        };
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
            _periodicProgressLogic = _periodicProgressLogic.Update(DateTime.Now);
            progressReporter.Report(_periodicProgressLogic.CurrentProgress);
        }
    }
}
