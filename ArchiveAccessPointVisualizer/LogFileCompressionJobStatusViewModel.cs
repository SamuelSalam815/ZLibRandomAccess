using System.ComponentModel;
using System.Runtime.CompilerServices;
using ErrorOr;
using KellermanSoftware.CompareNetObjects;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressionJobStatusViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private LogFileCompressionJobStatus _jobStatus = LogFileCompressionJobStatus.NotReady;

    private readonly CompareLogic _compareLogic = new(new ComparisonConfig
    {
        MaxDifferences = int.MaxValue,
    });

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateModel(ErrorOr<LogFileCompressionProgressReport> progressReport)
    {
        UpdateModel(_jobStatus.Accept(progressReport));
    }

    public void UpdateModel(Func<LogFileCompressionJobStatus, LogFileCompressionJobStatus> mutator)
    {
        UpdateModel(mutator(_jobStatus));
    }

    public void UpdateModel(LogFileCompressionJobStatus newJobStatus)
    {
        var comparison = _compareLogic.Compare(_jobStatus, newJobStatus);
        var previousButtonText = BeginCompressionJobButtonLabelOrProgressString;

        _jobStatus = newJobStatus;

        foreach (var difference in comparison.Differences)
        {
            OnPropertyChanged(difference.PropertyName);
        }

        // Note: we need to check this property explicitly because it doesn't live on the domain object, so the compare logic will not catch the differences.
        if (BeginCompressionJobButtonLabelOrProgressString != previousButtonText)
        {
            OnPropertyChanged(nameof(BeginCompressionJobButtonLabelOrProgressString));
        }
    }

    public bool IsRunning => _jobStatus.IsRunning;

    public string JobStatusDescription => _jobStatus.JobStatusDescription;

    public long NumberOfBytesWritten => _jobStatus.NumberOfBytesWritten;

    public long TargetNumberOfBytes => _jobStatus.TargetNumberOfBytes;

    public const string StartButtonLabelString = "Start";

    public string BeginCompressionJobButtonLabelOrProgressString
    {
        get
        {
            if (!IsRunning)
            {
                return StartButtonLabelString;
            }

            var ratio = (double)NumberOfBytesWritten / TargetNumberOfBytes;
            var percentage = ratio * 100;
            var roundedPercentage = Math.Round(percentage, 2, MidpointRounding.AwayFromZero);
            return $"{roundedPercentage:N2}%";
        }
    }
}
