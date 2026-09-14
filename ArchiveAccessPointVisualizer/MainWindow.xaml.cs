using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace ArchiveAccessPointVisualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private CancellationTokenSource _logCompressionCancellationTokenSource = new();

    private async void CompressLogFileButton_Click(object sender, RoutedEventArgs e)
    {
        CompressLogFileButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        CompressionJobStatusTextBlock.Text = "Generating fictitious logs...";
        LogFileCompressionJob? job = null;
        try
        {
            if (!_logCompressionCancellationTokenSource.TryReset())
            {
                _logCompressionCancellationTokenSource = new CancellationTokenSource();
            }

            if (!BrowseForOutputFile(out var filePath))
            {
                CompressionJobStatusTextBlock.Text = "";
                return;
            }

            job = CreateLogCompressionJob(filePath);
            await RunLogCompressionJobAsync(job, _logCompressionCancellationTokenSource.Token);
            CompressionJobStatusTextBlock.Text = "Finished generating fictitious logs!";
        }
        catch (OperationCanceledException)
        {
            CompressionJobStatusTextBlock.Text = "Cancelled log generation!";
            CleanPartiallyWrittenFiles(job);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            CompressLogFileButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            CompressLogFileButton.Content = "Write Log File";
        }
    }

    private void CleanPartiallyWrittenFiles(LogFileCompressionJob? job)
    {
        if (job is null)
        {
            return;
        }

        try
        {
            TryDeleteFile(job.OutputFilePath);

            if (job.RecoveryPointFilePath is not null)
            {
                TryDeleteFile(job.RecoveryPointFilePath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (IOException)
        {
        }
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

    private async Task RunLogCompressionJobAsync(LogFileCompressionJob job, CancellationToken cancellationToken = default)
    {
        var dataCompressedProgress = new Progress<long>();
        dataCompressedProgress.ProgressChanged += (_, totalProgress) =>
        {
            LogCompressionProgressBar.Value = totalProgress;
            var fractionalProgress = (double)totalProgress / job.RequestedUncompressedLogSizeInBytes * 100;
            CompressLogFileButton.Content = $"{fractionalProgress:N2}%";
        };

        var gameSimProgress = new Progress<string>();
        gameSimProgress.ProgressChanged += (_, logLine) => CompressionJobStatusTextBlock.Text = logLine;

        await new LogFileCompressor().Run(job, dataCompressedProgress, gameSimProgress, cancellationToken);
    }

    // TODO: provide recovery point writing toggle
    private LogFileCompressionJob CreateLogCompressionJob(string outputFilePath)
    {
        var targetLogSizeUnitless = long.Parse(LogSizeTargetTextBox.Text);
        var logSizeUnit = LogSizeUnitComboBox.Text switch
        {
            "GB" => 1024 * 1024 * 1024,
            "MB" => 1024 * 1024,
            _ => throw new InvalidOperationException($"Unexpected ComboBox string '{LogSizeUnitComboBox.Text}'!"),
        };
        var targetLogSizeBytes = targetLogSizeUnitless * logSizeUnit;
        LogCompressionProgressBar.Maximum = targetLogSizeBytes;
        return new LogFileCompressionJob(outputFilePath, targetLogSizeBytes, Encoding.Default);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _logCompressionCancellationTokenSource.Cancel();
    }
}
