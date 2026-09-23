using System.Collections.Immutable;
using System.IO;
using System.Text;
using ErrorOr;

namespace ArchiveAccessPointVisualizer;

public record LogFileCompressionRequestBuilder
{
    public static readonly ImmutableList<DataSize> AvailableUnitsOfData =
    [
        DataSize.FromMegaBytes(1),
        DataSize.FromGigaBytes(1),
    ];

    public string OutputFilePath { get; init; } = string.Empty;

    public string UserDefinedRecoveryPointFilePath { get; init; } = string.Empty;

    public string DerivedRecoveryPointFilePath => Path.ChangeExtension(OutputFilePath, DefaultRecoveryPointFileExtension);


    public bool ShouldUseRecoveryPoints { get; init; }

    public bool ShouldDeriveRecoveryPointFilePath { get; init; } = true;

    public string RequestedLogFileSizeString { get; init; } = "512";

    public ErrorOr<DataSize> RequestedLogFileSize
    {
        get
        {
            if (long.TryParse(RequestedLogFileSizeString, out var requestedLogFileSize))
            {
                return new DataSize(requestedLogFileSize * LogSizeUnitOfMeasure.ByteCount);
            }

            return Error.Validation("Could not parse requested file size as an integer!");
        }
    }

    public DataSize LogSizeUnitOfMeasure { get; init; } = AvailableUnitsOfData.First();

    public const string DefaultOutputFileExtenstion = ".txt.gz";

    public const string DefaultRecoveryPointFileExtension = ".recovery_points.csv";

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

    public ErrorOr<LogFileCompressionRequest> Request
    {
        get
        {
            return RequestedLogFileSize
                .Then(dataSize =>
                {
                    var recoveryPointFileDetails = RecoveryPointFilePath == string.Empty
                        ? null
                        : new RecoveryPointFileDetails(RecoveryPointFilePath, DataSize.FromMegaBytes(1));

                    return new LogFileCompressionRequest(
                        OutputFilePath,
                        recoveryPointFileDetails,
                        Encoding.Default,
                        dataSize
                    );
                });
        }
    }
}
