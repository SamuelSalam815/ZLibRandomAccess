namespace LogSimulator.Appliction;

public record CompressionFilePathFactory(DirectoryInfo BaseOutputDirectory, DateTimeOffset Timestamp)
{
    public FileInfo BCLCompressionFile =>
        new(FromBaseDirectory($"{TimestampString} SimulatedLogs BCL Compression.log.gz"));
    public FileInfo ZLibCompressionFile =>
        new(FromBaseDirectory($"{TimestampString} SimulatedLogs zlib Compression.log.gz"));
    public FileInfo ZLibCompressionWithRecoveryPointsFile =>
        new(FromBaseDirectory($"{TimestampString} SimulatedLogs zlib Compression with Recovery Points.log.gz"));

    public FileInfo RecoveryPointFile => new(ZLibCompressionWithRecoveryPointsFile + RecoveryPointExtension);
    private static string RecoveryPointExtension => ".recovery_offsets.csv";
    public string TimestampString => $"{Timestamp:yyyy-MM-ddTHH.mm.ss}";

    public DirectoryInfo TimestampedOutputDirectory => new(Path.Combine(BaseOutputDirectory.FullName, TimestampString));
    private string FromBaseDirectory(string fileName) =>
        Path.Combine(TimestampedOutputDirectory.FullName, fileName);
}
