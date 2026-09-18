using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArchiveAccessPointVisualizer;

public class LogGenerationViewModel : INotifyPropertyChanged, ILogGenerationProperties
{
    private LogGenerationFormValues _values = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanges(LogGenerationFormValues newValues)
    {
        var oldValues = _values;
        _values = newValues;
        var changedProperties = oldValues.GetChangedProperties(_values);
        foreach (var changedProperty in changedProperties)
        {
            OnPropertyChanged(changedProperty);
        }
    }

    public string OutputFilePath
    {
        get => _values.OutputFilePath;
        set => NotifyPropertyChanges(_values with { OutputFilePath = value });
    }

    public string UserDefinedRecoveryPointFilePath
    {
        get => _values.UserDefinedRecoveryPointFilePath;
        set => NotifyPropertyChanges(_values with { UserDefinedRecoveryPointFilePath = value });
    }

    public string DerivedRecoveryPointFilePath => _values.DerivedRecoveryPointFilePath;

    public string RecoveryPointFilePath => _values.RecoveryPointFilePath;

    public bool ShouldUseRecoveryPoints
    {
        get => _values.ShouldUseRecoveryPoints;
        set => NotifyPropertyChanges(_values with { ShouldUseRecoveryPoints = value });
    }
    public bool ShouldDeriveRecoveryPointFilePath
    {
        get => _values.ShouldDeriveRecoveryPointFilePath;
        set => NotifyPropertyChanges(_values with { ShouldDeriveRecoveryPointFilePath = value });
    }

    public bool CanUserSpecifyRecoveryPointFilePath => _values.CanUserSpecifyRecoveryPointFilePath;

    public string RequestedLogFileSize
    {
        get => _values.RequestedLogFileSize;
        set => NotifyPropertyChanges(_values with { RequestedLogFileSize = value });
    }

    public string LogSizeUnitOfMeasure
    {
        get => _values.LogSizeUnitOfMeasure;
        set => NotifyPropertyChanges(_values with { LogSizeUnitOfMeasure = value });
    }
}
