using System.IO.Compression;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.TextSearching;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using JetBrains.Annotations;
using LogSimulator.Simulator;
using ZLibWrapper;

namespace Benchmarking.TextSearch;

[SimpleJob(RunStrategy.Monitoring)]
[CsvMeasurementsExporter]
[ReturnValueValidator(true)]
public class BenchmarkingRegexInGzipStream
{
    [ParamsSource(nameof(ValuesForUncompressedLogSizeInMegabytes))]
    public static int UncompressedLogSizeInMegabytes { get; set; } = 32;

    // public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [512, 2048, 8192];
    public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [32];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private const long RecoveryPointByteInterval = DataSize.MegaByte;
    private const long ParallelZlibGzipOverlapInBytes = 3 * DataSize.KiloByte;

    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];

    private ScanTextSearcher? SystemGzipSearcher;
    private ScanTextSearcher? ZlibGzipSearcher;
    private ParallelScanTextSearcher? ZlibGzipParallelSearcher;

    private readonly string _creationTime = DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss zz");

    private FileInfo LogFile => new($"{_creationTime} SimulatedLogs.log");

    private FileInfo SystemGzipLogFile => new($"{_creationTime} SystemGzip.gzip");

    private FileInfo ZlibGzipLogFile => new($"{_creationTime} ZlibGzip.gzip");

    private FileInfo ZlibGzipRecoveryLogFile => new($"{_creationTime} ZlibGzipRecovery.gzip");

    [MustDisposeResource]
    private static FileStream OpenNew(FileInfo file)
    {
        return file.Create(FileMode.CreateNew, FileSystemRights.Write, FileShare.Read, 1024, FileOptions.None, null);
    }

    [MustDisposeResource]
    private static FileStream OpenRead(FileInfo file)
    {
        return file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    [MustDisposeResource]
    private static StreamWithDisposeEvent OpenReadAndDeleteOnDisposal(FileInfo file)
    {
        var cleanUpStream = new StreamWithDisposeEvent(OpenRead(file));

        cleanUpStream.OnDisposed += file.Delete;
        return cleanUpStream;
    }

    [GlobalSetup]
    public void GlobalSetUp()
    {
        WriteUncompressedLogFile();
        WriteCompressedLogFiles();

        SystemGzipSearcher = new ScanTextSearcher(new GZipStream(OpenReadAndDeleteOnDisposal(SystemGzipLogFile), CompressionMode.Decompress), Encoding);
        ZlibGzipSearcher = new ScanTextSearcher(new GZipReadingStreamWithRecoveryPoints(OpenReadAndDeleteOnDisposal(ZlibGzipLogFile)), Encoding);
        ZlibGzipParallelSearcher = TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                OpenReadAndDeleteOnDisposal(ZlibGzipLogFile),
                recoveryPointOffsets: _recoveryPointOffsets),
            _recoveryPointOffsets,
            ParallelZlibGzipOverlapInBytes);
    }

    private void WriteUncompressedLogFile()
    {
        var sim = new LogSimulator.Simulator.LogSimulator();
        using var logFileStream = OpenNew(LogFile);
        sim.SimulateLogs(logFileStream, Encoding, UncompressedLogSizeInMegabytes * DataSize.MegaByte);
    }

    private void WriteCompressedLogFiles()
    {
        if (!SystemGzipLogFile.Exists)
        {
            using var outputStream = OpenNew(SystemGzipLogFile);
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipStream(outputStream, CompressionMode.Compress);
            logFileStream.CopyTo(compressor);
        }

        if (!ZlibGzipLogFile.Exists)
        {
            using var outputStream = OpenNew(ZlibGzipLogFile);
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipWritingStreamWithRecoveryPoints(outputStream);
            logFileStream.CopyTo(compressor);
        }

        if (!ZlibGzipRecoveryLogFile.Exists)
        {
            using var outputStream = OpenNew(ZlibGzipRecoveryLogFile);
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipWritingStreamWithRecoveryPoints(
                outputStream,
                recoveryPointByteInterval: RecoveryPointByteInterval);
            compressor.RecoveryPointWritten += _recoveryPointOffsets.Add;
            logFileStream.CopyTo(compressor);
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        SystemGzipSearcher?.Dispose();
        ZlibGzipSearcher?.Dispose();
        ZlibGzipParallelSearcher?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll() => SystemGzipSearcher!.FindAll(SearchPattern).ToList();

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll() => ZlibGzipSearcher!.FindAll(SearchPattern).ToList();

    // [Benchmark]
    // public List<SearchResult> ZlibGzip_Parallel_FindAll() => ZlibGzipParallelSearcher!.FindAll(SearchPattern).ToList();
}
