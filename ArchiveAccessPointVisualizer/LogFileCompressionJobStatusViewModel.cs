using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    public void UpdateModel(LogFileCompressionJobStatus newJobStatus)
    {
        var comparison = _compareLogic.Compare(_jobStatus, newJobStatus);
        var previousButtonText = BeginCompressionJobButtonLabelOrProgressText;

        _jobStatus = newJobStatus;

        foreach (var difference in comparison.Differences)
        {
            OnPropertyChanged(difference.PropertyName);
        }

        // Note: we need to check this property explicitly because it doesn't live on the domain object, so the compare logic will not catch the differences.
        if (BeginCompressionJobButtonLabelOrProgressText != previousButtonText)
        {
            OnPropertyChanged(nameof(BeginCompressionJobButtonLabelOrProgressText));
        }
    }

    public bool IsRunning => _jobStatus.IsRunning;

    public string JobStatusDescription => _jobStatus.Description;

    public long NumberOfBytesWritten => _jobStatus.NumberOfBytesWritten;

    public long TargetNumberOfBytes => _jobStatus.TargetNumberOfBytes;

    // TODO: when job is running, make this display the percent progress
    public string BeginCompressionJobButtonLabelOrProgressText => "Start";
}
