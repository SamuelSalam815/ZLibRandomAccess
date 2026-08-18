using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.TextSearching;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ZLibWrapper;

namespace Benchmarking;

[SimpleJob(RunStrategy.Monitoring)]
public class RegexInGzipStream
{
    [ParamsSource(nameof(ValuesForUncompressedLogMegabyteCount))]
    public int UncompressedLogMegabyteCount { get; set; }

    public static IEnumerable<int> ValuesForUncompressedLogMegabyteCount => [4_096, 8_192, 16_384, 32_768, 65_536];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private const long MegaByte = 1024 * 1024;

    private readonly MemoryStream _compressedData = new();
    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];
    private ITextSearcher? _textSearcher;

    [GlobalSetup(Target = nameof(SystemGzip_FindAll))]
    public void GlobalSetUpSystemGzip()
    {
        GenerateCompressedData(outputStream => new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true));
    }

    [GlobalSetup(Target = nameof(ZlibGzip_FindAll))]
    public void GlobalSetUpZlibGzip()
    {
        GenerateCompressedData(outputStream => new GZipWritingStreamWithRecoveryPoints(outputStream, leaveOpen: true));
    }

    [GlobalSetup(Target = nameof(ZlibGzip_Parallel_FindAll))]
    public void GlobalSetUpZlibGzipParallel()
    {
        GenerateCompressedData(outputStream =>
        {
            var result = new GZipWritingStreamWithRecoveryPoints(
                outputStream,
                leaveOpen: true,
                recoveryPointByteInterval: MegaByte);
            result.RecoveryPointWritten += _recoveryPointOffsets.Add;
            return result;
        });
    }

    private void GenerateCompressedData(Func<Stream, Stream> compressorFactory)
    {
        var sim = new LogSimulator.Simulator.LogSimulator();
        using (var compressor = compressorFactory(_compressedData))
        {
            sim.SimulateLogs(compressor, Encoding, UncompressedLogMegabyteCount * MegaByte);
        }
        _compressedData.Seek(0, SeekOrigin.Begin);
    }

    [IterationSetup(Target = nameof(SystemGzip_FindAll))]
    public void IterationSetUpSystemGzip()
    {
        var decompressor  = new GZipStream(_compressedData, CompressionMode.Decompress, leaveOpen: true);
        _textSearcher = new ScanTextSearcher(decompressor, Encoding);
    }

    [IterationSetup(Target = nameof(ZlibGzip_FindAll))]
    public void IterationSetUpZlibGzip()
    {
        var decompressor  = new GZipReadingStreamWithRecoveryPoints(_compressedData, leaveOpen: true);
        _textSearcher = new ScanTextSearcher(decompressor, Encoding);
    }

    [IterationSetup(Target = nameof(ZlibGzip_Parallel_FindAll))]
    public void IterationSetUpZlibGzipParallel()
    {
        _textSearcher = TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                _compressedData,
                leaveOpen: true,
                recoveryPointOffsets: _recoveryPointOffsets),
            _recoveryPointOffsets);
    }

    [IterationCleanup]
    public void IterationCleanUp()
    {
        _textSearcher?.Dispose();
    }

    [GlobalCleanup]
    public void GlobalCleanUp()
    {
        _compressedData.Dispose();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();

    [Benchmark]
    public List<SearchResult> ZlibGzip_Parallel_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();
}
