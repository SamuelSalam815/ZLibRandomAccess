using System.ComponentModel;
using System.Runtime.CompilerServices;
using KellermanSoftware.CompareNetObjects;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressionJobViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly CompareLogic _compareLogic = new(new ComparisonConfig
    {
        MaxDifferences = int.MaxValue,
    });

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }



    public async Task StartAsync(LogFileCompressionRequest request)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
