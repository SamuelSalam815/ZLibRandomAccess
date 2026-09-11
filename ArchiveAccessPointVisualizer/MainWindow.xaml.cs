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

    private readonly CancellationTokenSource _logCompressionCancellationTokenSource = new();

    private async void CompressLogFileButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = "CompressedLogFile.txt", // Default file name
                DefaultExt = ".gz", // Default file extension
                Filter = "Compressed Log Files and Recovery Points|*.gz;*.csv" // Filter files by extension
            };

            if (dialog.ShowDialog() is not true)
            {
                return;
            }

            var filename = dialog.FileName;
            var targetLogSizeUnitless = long.Parse(LogSizeTargetTextBox.Text);
            var logSizeUnit = LogSizeUnitComboBox.Text switch
            {
                "GB" => 1024 * 1024 * 1024,
                "MB" => 1024 * 1024,
                _ => throw new InvalidOperationException($"Unexpected ComboBox string '{LogSizeUnitComboBox.Text}'!"),
            };
            var targetLogSizeBytes = targetLogSizeUnitless * logSizeUnit;
            LogCompressionProgressBar.Maximum = targetLogSizeBytes;
            var job = new LogFileCompressionJob(filename, targetLogSizeBytes, Encoding.Default);

            await RunLogGenerationJob(job);
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RunLogGenerationJob(LogFileCompressionJob job)
    {
        var progress = new Progress<long>();
        progress.ProgressChanged += (progressSender, totalProgress) =>
            LogCompressionProgressBar.Value = totalProgress;

        if (!_logCompressionCancellationTokenSource.TryReset())
        {
            throw new Exception("Unexpectedly failed to reset cancellation token!");
        }
        CompressLogFileButton.IsEnabled = true;
        CancelButton.IsEnabled = true;
        await Task.Run(() => new LogFileCompressor().Run(job, progress, _logCompressionCancellationTokenSource.Token)
            .ContinueWith(_ =>
            {
                CompressLogFileButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }));
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _logCompressionCancellationTokenSource.Cancel();
    }
}
