using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.TextSearching;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ZLibWrapper;

namespace Benchmarking;

[SimpleJob(RunStrategy.Monitoring)]
public class BenchmarkingRegexInGzipStream
{
    [ParamsSource(nameof(ValuesForUncompressedLogSizeInMegabytes))]
    public int UncompressedLogSizeInMegabytes { get; set; }

    public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [64, 128, 256, 512, 1024];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private const long KiloByte = 1024;
    private const long MegaByte = KiloByte * KiloByte;
    private const long RecoveryPointByteInterval = MegaByte;
    private const long ParallelZlibGzipOverlapInBytes = 3 * KiloByte;

    private byte[] _compressedData = [];
    private readonly List<RecoveryPointOffset> _recoveryPointOffsets = [];
    private ITextSearcher? _textSearcher;

    private Stream GetCompressedDataStream() => new MemoryStream(_compressedData, writable: false);

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
                RecoveryPointByteInterval);
            result.RecoveryPointWritten += _recoveryPointOffsets.Add;
            return result;
        });
    }

    private void GenerateCompressedData(Func<Stream, Stream> compressorFactory)
    {
        var sim = new LogSimulator.Simulator.LogSimulator();
        using var compressedDataStream = new MemoryStream();
        using (var compressor = compressorFactory(compressedDataStream))
        {
            sim.SimulateLogs(compressor, Encoding, UncompressedLogSizeInMegabytes * MegaByte);
        }

        _compressedData = compressedDataStream.ToArray();
    }

    [IterationSetup(Target = nameof(SystemGzip_FindAll))]
    public void IterationSetUpSystemGzip()
    {
        var decompressor  = new GZipStream(GetCompressedDataStream(), CompressionMode.Decompress);
        _textSearcher = new ScanTextSearcher(decompressor, Encoding);
    }

    [IterationSetup(Target = nameof(ZlibGzip_FindAll))]
    public void IterationSetUpZlibGzip()
    {
        var decompressor  = new GZipReadingStreamWithRecoveryPoints(GetCompressedDataStream());
        _textSearcher = new ScanTextSearcher(decompressor, Encoding);
    }

    [IterationSetup(Target = nameof(ZlibGzip_Parallel_FindAll))]
    public void IterationSetUpZlibGzipParallel()
    {
        _textSearcher = TextSearcherFactory.CreateParallelTextSearcher(
            () => new GZipReadingStreamWithRecoveryPoints(
                GetCompressedDataStream(),
                recoveryPointOffsets: _recoveryPointOffsets),
            _recoveryPointOffsets,
            ParallelZlibGzipOverlapInBytes);
    }

    [IterationCleanup]
    public void IterationCleanUp()
    {
        _textSearcher?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();

    [Benchmark]
    public List<SearchResult> ZlibGzip_Parallel_FindAll() => _textSearcher!.FindAll(SearchPattern).ToList();
}
