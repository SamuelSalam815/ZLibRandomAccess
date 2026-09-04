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

// [SimpleJob(RunStrategy.Monitoring)]
[CsvMeasurementsExporter]
// [ReturnValueValidator(true)]
public class BenchmarkingRegexInGzipStream
{
    // [ParamsSource(nameof(ValuesForUncompressedLogSizeInMegabytes))]
    public static int UncompressedLogSizeInMegabytes { get; set; } = ValuesForUncompressedLogSizeInMegabytes.First();

    // public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [512, 2048, 8192];
    public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [10];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];

    private ScanTextSearcher? SystemGzipSearcher;
    private ScanTextSearcher? ZlibGzipSearcher;
    private ParallelScanTextSearcher? ZlibGzipParallelSearcher;

    private readonly string _creationTime = DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss zz");

    private FileInfo LogFile => new($"{_creationTime} SimulatedLogs.log");

    private FileInfo SystemGzipLogFile => new($"{_creationTime} SystemGzip.gzip");

    private FileInfo ZlibGzipLogFile => new($"{_creationTime} ZlibGzip.gzip");

    private FileInfo ZlibGzipLogFileWithRecoveryPoints => new($"{_creationTime} ZlibGzipRecovery.gzip");

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

    [GlobalSetup]
    public void GlobalSetUp()
    {
        WriteUncompressedLogFile();
        WriteCompressedLogFiles();

        SystemGzipSearcher = new ScanTextSearcher(new GZipStream(OpenRead(SystemGzipLogFile), CompressionMode.Decompress), Encoding);
        ZlibGzipSearcher = new ScanTextSearcher(new GZipReadingStreamWithRecoveryPoints(OpenRead(ZlibGzipLogFile)), Encoding);
        ZlibGzipParallelSearcher = TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                OpenRead(ZlibGzipLogFileWithRecoveryPoints),
                recoveryPointOffsets: _recoveryPointOffsets),
            _recoveryPointOffsets,
            parallelStreamOverlapInBytes: DataSize.KiloByte);
    }

    private void WriteUncompressedLogFile()
    {
        var sim = new LogSimulator.Simulator.LogSimulator();
        using var logFileStream = OpenNew(LogFile);
        sim.SimulateLogs(logFileStream, Encoding, UncompressedLogSizeInMegabytes * DataSize.MegaByte);
    }

    private void WriteCompressedLogFiles()
    {
        using (var outputStream = OpenNew(SystemGzipLogFile))
        {
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipStream(outputStream, CompressionMode.Compress);
            logFileStream.CopyTo(compressor);
        }

        using (var outputStream = OpenNew(ZlibGzipLogFile))
        {
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipWritingStreamWithRecoveryPoints(outputStream);
            logFileStream.CopyTo(compressor);
        }

        using (var outputStream = OpenNew(ZlibGzipLogFileWithRecoveryPoints))
        {
            using var logFileStream = LogFile.OpenRead();
            using var compressor = new GZipWritingStreamWithRecoveryPoints(
                outputStream,
                recoveryPointByteInterval: DataSize.MegaByte);
            compressor.RecoveryPointWritten += _recoveryPointOffsets.Add;
            logFileStream.CopyTo(compressor);
        }

    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        LogFile.Delete();
        SystemGzipSearcher?.Dispose();
        ZlibGzipSearcher?.Dispose();
        ZlibGzipParallelSearcher?.Dispose();

        SystemGzipLogFile.Delete();
        ZlibGzipLogFile.Delete();
        ZlibGzipLogFileWithRecoveryPoints.Delete();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll()
    {
        var result = SystemGzipSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll()
    {
        var result = ZlibGzipSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }

    [Benchmark]
    public List<SearchResult> ZlibGzip_Parallel_FindAll()
    {
        var result = ZlibGzipParallelSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }
}
