using System.IO;
using System.Text;
using ZLibWrapper;

namespace ArchiveAccessPointVisualizer;

// Core
public record LogFileCompressionJob
{
    public LogFileCompressionJob(string OutputFilePath, long RequestedLogSize, Encoding Encoding, long? RecoveryPointByteInterval = null)
    {
        if (Path.GetExtension(OutputFilePath) != ".gz")
        {
            throw new ArgumentException("Output file path is not to a gz file!", nameof(OutputFilePath));
        }

        this.OutputFilePath = OutputFilePath;
        this.RequestedLogSize = RequestedLogSize;
        this.Encoding = Encoding;
        this.RecoveryPointByteInterval = RecoveryPointByteInterval;

        if (RecoveryPointByteInterval is not null)
        {
            RecoveryPointFilePath = Path.ChangeExtension(OutputFilePath, "csv");
        }
    }

    public string OutputFilePath { get; }
    public string? RecoveryPointFilePath { get; }
    public long RequestedLogSize { get; }
    public Encoding Encoding { get; }
    public long? RecoveryPointByteInterval { get; }
}

// Shell
public class LogFileCompressor
{
    public async Task Run(
        LogFileCompressionJob job,
        IProgress<long> numberOfBytesWrittenReporter,
        CancellationToken cancellationToken
    )
    {
        await using var outputFileStream = File.Open(job.OutputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        await using var recoveryPointFileStream = job.RecoveryPointFilePath is null
            ? Stream.Null
            : File.Open(job.RecoveryPointFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var compressor = new GZipWritingStreamWithRecoveryPoints(outputFileStream, recoveryPointByteInterval: job.RecoveryPointByteInterval);
        await using var accessPointStream = new RecoveryPointOffsetCsvWriter(recoveryPointFileStream, job.Encoding);
        compressor.RecoveryPointWritten += accessPointStream.Write;
        var logSim = new LogSimulator.Simulator.LogSimulator();
        await logSim.SimulateLogsAsync(
            compressor,
            job.Encoding,
            job.RequestedLogSize,
            numberOfBytesWrittenReporter,
            cancellationToken);
    }
}
