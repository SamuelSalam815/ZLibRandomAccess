using System.Windows;
using ErrorOr;
using Microsoft.Win32;

namespace ArchiveAccessPointVisualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private LogFileCompressionRequestBuilderViewModel RequestBuilderViewModel => (FindResource("JobRequestModel") as LogFileCompressionRequestBuilderViewModel)!;
    private LogFileCompressionJobStatusViewModel JobStatusViewModel => (FindResource("JobStatusModel") as LogFileCompressionJobStatusViewModel)!;

    public MainWindow()
    {
        InitializeComponent();
        // TODO: replace this with a collection that implements INotifyCollectionChanged
        LogSizeUnitComboBox.ItemsSource =
            LogFileCompressionRequestBuilder.AvailableUnitsOfData.Select(x => new DataSizeAsUnitString(x));
    }

    private CancellationTokenSource _logCompressionCancellationTokenSource = new();

    private void SeekOutputFilePath(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog()
        {
            AddExtension = true,
            DefaultExt = ".txt.gz",
            Filter = "Archives and Metadata Files (*.gz; *.csv)|*.gz;*.csv",
            Title = "Select the Archive File Path"
        };

        if (dialog.ShowDialog() is true)
        {
            RequestBuilderViewModel.OutputFilePath = dialog.FileName;
        }
    }

    private void SeekRecoveryPointFilePath(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog()
        {
            AddExtension = true,
            DefaultExt = ".csv",
            Filter = "Metadata Files and Archives (*.csv; *.gz)|*.csv;*.gz",
            Title = "Select the Metadata File Path"
        };

        if (dialog.ShowDialog() is true)
        {
            RequestBuilderViewModel.UserDefinedRecoveryPointFilePath = dialog.FileName;
        }
    }

    private async void StartNewCompressionJob_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await RequestBuilderViewModel.RequestOrError
                .ThenDoAsync(async request =>
                {
                    _logCompressionCancellationTokenSource = new CancellationTokenSource();
                    var jobRunner = new LogFileCompressionJobRunner(request);
                    var progress = new Progress<ErrorOr<LogFileCompressionProgressReport>>();
                    progress.ProgressChanged += (_, report) => JobStatusViewModel.UpdateModel(report);
                    JobStatusViewModel.UpdateModel(model =>
                        model with { TargetNumberOfBytes = request.RequestedLogSize });
                    await jobRunner.Run(
                        progress,
                        _logCompressionCancellationTokenSource.Token
                    );
                })
                .ElseDoAsync(
                    error =>
                    {
                        JobStatusViewModel.UpdateModel(error);
                        return Task.CompletedTask;
                    }
                );
        }
        catch (Exception exception)
        {
            MessageBox.Show("Unhandled Exception! " + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelCompressionJob_Click(object sender, RoutedEventArgs e)
    {
        _logCompressionCancellationTokenSource.Cancel();
    }
}
