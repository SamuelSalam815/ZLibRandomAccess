using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using ErrorOr;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Win32;

namespace ArchiveAccessPointVisualizer;

public class LogFileCompressionRequestBuilderViewModel : INotifyPropertyChanged
{
    private LogFileCompressionRequestBuilder _requestBuilder = new();

    public ErrorOr<LogFileCompressionRequest> Request => _requestBuilder.Request;

    private readonly CompareLogic _compareLogic = new(new ComparisonConfig
    {
        MaxDifferences = int.MaxValue,
    });

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateModel(LogFileCompressionRequestBuilder newModel)
    {
        var comparison = _compareLogic.Compare(_requestBuilder, newModel);
        _requestBuilder = newModel;
        foreach (var difference in comparison.Differences)
        {
            OnPropertyChanged(difference.PropertyName);
        }
    }

    public string OutputFilePath
    {
        get => _requestBuilder.OutputFilePath;
        set => UpdateModel(_requestBuilder with { OutputFilePath = value });
    }

    public string UserDefinedRecoveryPointFilePath
    {
        get => _requestBuilder.UserDefinedRecoveryPointFilePath;
        set => UpdateModel(_requestBuilder with { UserDefinedRecoveryPointFilePath = value });
    }

    public string RecoveryPointFilePath => _requestBuilder.RecoveryPointFilePath;

    public bool ShouldUseRecoveryPoints
    {
        get => _requestBuilder.ShouldUseRecoveryPoints;
        set => UpdateModel(_requestBuilder with { ShouldUseRecoveryPoints = value });
    }

    public bool ShouldDeriveRecoveryPointFilePath
    {
        get => _requestBuilder.ShouldDeriveRecoveryPointFilePath;
        set => UpdateModel(_requestBuilder with { ShouldDeriveRecoveryPointFilePath = value });
    }

    public bool CanUserSpecifyRecoveryPointFilePath => _requestBuilder is { ShouldUseRecoveryPoints: true, ShouldDeriveRecoveryPointFilePath: false };

    public string RequestedLogFileSize
    {
        get => _requestBuilder.RequestedLogFileSizeString;
        set => UpdateModel(_requestBuilder with { RequestedLogFileSizeString = value });
    }

    private static bool BrowseForOutputFile([NotNullWhen(true)]out string? outputFilePath)
    {
        var dialog = new SaveFileDialog
        {
            FileName = "CompressedLogFile.txt.gz",
            Filter = "Compressed Log Files and Recovery Points|*.gz;*.csv" // Filter files by extension
        };

        if (dialog.ShowDialog() is true)
        {
            outputFilePath = dialog.FileName;
            return true;
        }

        outputFilePath = null;
        return false;
    }

    private void SeekOutputFilePath(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog()
        {
            AddExtension = true,
            DefaultExt = ".txt.gz",
            Filter = "Archive|*.gz",
            Title = "Generate a Log File"
        };

        if (dialog.ShowDialog() is true)
        {
            UpdateModel(_requestBuilder with { OutputFilePath = dialog.FileName });
        }
    }

    private void SeekRecoveryPointFilePath(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog()
        {
            AddExtension = true,
            DefaultExt = ".csv",
            Filter = "Metadata File|*.csv",
            Title = "Choose the metadata output path"
        };

        if (dialog.ShowDialog() is true)
        {
            UpdateModel(_requestBuilder with { UserDefinedRecoveryPointFilePath = dialog.FileName });
        }
    }

}
