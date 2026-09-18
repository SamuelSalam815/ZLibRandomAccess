using System.IO;

namespace ArchiveAccessPointVisualizer;

public record LogGenerationFormValues : ILogGenerationProperties
{
    public string OutputFilePath { get; init; } = string.Empty;
    public string UserDefinedRecoveryPointFilePath { get; init; } = string.Empty;
    public string DerivedRecoveryPointFilePath => Path.ChangeExtension(
        OutputFilePath,
        ILogGenerationProperties.DefaultRecoveryPointFileExtension);

    public string RecoveryPointFilePath
    {
        get
        {
            if (!ShouldUseRecoveryPoints)
            {
                return string.Empty;
            }

            return ShouldDeriveRecoveryPointFilePath ? DerivedRecoveryPointFilePath : UserDefinedRecoveryPointFilePath;
        }
    }

    public bool ShouldUseRecoveryPoints { get; init; } = false;
    public bool ShouldDeriveRecoveryPointFilePath { get; init; } = true;
    public bool CanUserSpecifyRecoveryPointFilePath => ShouldUseRecoveryPoints && !ShouldDeriveRecoveryPointFilePath;

    public string RequestedLogFileSize { get; init; } = "512";
    public string LogSizeUnitOfMeasure { get; init; } = ILogGenerationProperties.MegabyteUnitOfMeasure;

    public IEnumerable<string> GetChangedProperties(LogGenerationFormValues @new)
    {
        var result = EnumerateChangedProperties(@new).ToList();
        return result;
    }

    private IEnumerable<string> EnumerateChangedProperties(LogGenerationFormValues @new)
    {
        if (OutputFilePath != @new.OutputFilePath)
        {
            yield return nameof(OutputFilePath);
        }

        if (UserDefinedRecoveryPointFilePath != @new.UserDefinedRecoveryPointFilePath)
        {
            yield return nameof(UserDefinedRecoveryPointFilePath);
        }

        if (DerivedRecoveryPointFilePath != @new.DerivedRecoveryPointFilePath)
        {
            yield return nameof(DerivedRecoveryPointFilePath);
        }

        if (RecoveryPointFilePath != @new.RecoveryPointFilePath)
        {
            yield return nameof(RecoveryPointFilePath);
        }

        if (ShouldUseRecoveryPoints != @new.ShouldUseRecoveryPoints)
        {
            yield return nameof(ShouldUseRecoveryPoints);
        }

        if (ShouldDeriveRecoveryPointFilePath != @new.ShouldDeriveRecoveryPointFilePath)
        {
            yield return nameof(ShouldDeriveRecoveryPointFilePath);
        }

        if (CanUserSpecifyRecoveryPointFilePath != @new.CanUserSpecifyRecoveryPointFilePath)
        {
            yield return nameof(CanUserSpecifyRecoveryPointFilePath);
        }

        if (RequestedLogFileSize != @new.RequestedLogFileSize)
        {
            yield return nameof(RequestedLogFileSize);
        }

        if (LogSizeUnitOfMeasure != @new.LogSizeUnitOfMeasure)
        {
            yield return nameof(LogSizeUnitOfMeasure);
        }
    }
}
