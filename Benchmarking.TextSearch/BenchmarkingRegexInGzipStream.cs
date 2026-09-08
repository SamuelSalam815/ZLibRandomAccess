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
[CsvExporter]
[HtmlExporter]
[PlainExporter]
[ReturnValueValidator(true)]
public class BenchmarkingRegexInGzipStream
{
    [ParamsSource(nameof(ValuesForUncompressedLogSizeInMegabytes))]
    public static int UncompressedLogSizeInMegabytes { get; set; } = ValuesForUncompressedLogSizeInMegabytes.First();

    public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [256, 512, 1024];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];

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
        SystemGzipLogFile.Delete();
        ZlibGzipLogFile.Delete();
        ZlibGzipLogFileWithRecoveryPoints.Delete();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll()
    {
        using var textSearcher = new ScanTextSearcher(new GZipStream(OpenRead(SystemGzipLogFile), CompressionMode.Decompress), Encoding);
        var result = textSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll()
    {
        using var textSearcher = new ScanTextSearcher(new GZipReadingStreamWithRecoveryPoints(OpenRead(ZlibGzipLogFile)), Encoding);
        var result = textSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }

    [Benchmark]
    public List<SearchResult> ZlibGzip_Parallel_FindAll()
    {
        using var textSearcher = TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                OpenRead(ZlibGzipLogFileWithRecoveryPoints),
                recoveryPointOffsets: _recoveryPointOffsets),
            _recoveryPointOffsets,
            parallelStreamOverlapInBytes: DataSize.KiloByte);
        var result = textSearcher!.FindAll(SearchPattern).ToList();
        return result;
    }
}
