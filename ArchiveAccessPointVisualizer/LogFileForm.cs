namespace ArchiveAccessPointVisualizer;

public record LogFileForm(
    string OutputFilePath,
    long? RecoveryPointByteInterval,
    string RecoverPointFileExtension,
    long TargetUncompressedLogFileSize
)
{
    public string RecoverPointOutputFile => throw new NotImplementedException();
};
