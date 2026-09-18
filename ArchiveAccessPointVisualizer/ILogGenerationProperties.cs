namespace ArchiveAccessPointVisualizer;

public interface ILogGenerationProperties
{
    public const string DefaultOutputFileExtenstion = ".txt.gz";
    public const string DefaultRecoveryPointFileExtension = ".recovery_points.csv";
    public const string GigabyteUnitOfMeasure = "GB";
    public const string MegabyteUnitOfMeasure = "MB";

    public string OutputFilePath { get; }

    public string UserDefinedRecoveryPointFilePath { get; }
    public string DerivedRecoveryPointFilePath { get; }
    public string RecoveryPointFilePath { get; }

    public bool ShouldUseRecoveryPoints { get; }
    public bool ShouldDeriveRecoveryPointFilePath { get; }
    public bool CanUserSpecifyRecoveryPointFilePath { get; }

    public string RequestedLogFileSize { get; }
    public string LogSizeUnitOfMeasure { get; }
}