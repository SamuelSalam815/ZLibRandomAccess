using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.TextSearching;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;
using ZLibWrapper;

namespace Benchmarking;



[Config(typeof(Config))]
public class BenchmarkingRegexInGzipStream
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Dry
                .WithPlatform(Platform.X64)
                .WithJit(Jit.RyuJit)
                .WithStrategy(RunStrategy.Monitoring)
            );
            AddValidator(ReturnValueValidator.FailOnError);
            // AddColumn(new NumberColumn("CompressedLogSizeInBytes", () => _compressedData.Length));
        }
    }

    [ParamsSource(nameof(ValuesForUncompressedLogSizeInMegabytes))]
    public int UncompressedLogSizeInMegabytes { get; set; }

    // public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [64, 128, 256, 512, 1024];
    public static IEnumerable<int> ValuesForUncompressedLogSizeInMegabytes => [64];

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    private static readonly Encoding Encoding = Encoding.UTF8;

    private const long KiloByte = 1024;
    private const long MegaByte = KiloByte * KiloByte;
    private const long RecoveryPointByteInterval = MegaByte;
    private const long ParallelZlibGzipOverlapInBytes = KiloByte;

    private record BenchmarkScenario(
        IGzipBenchmarkSetUpStrategy SetUpStrategy,
        byte[]? CompressedData,
        ITextSearcher? TextSearcher
    )
    {
        public BenchmarkScenario GenerateCompressedData(int uncompressedLogSizeInMegabytes)
        {

            var sim = new LogSimulator.Simulator.LogSimulator();
            using var compressedDataStream = new MemoryStream();
            using (var compressor = SetUpStrategy.CreateCompressingStream(compressedDataStream))
            {
                sim.SimulateLogs(compressor, Encoding, uncompressedLogSizeInMegabytes * MegaByte);
            }

            return this with { CompressedData = compressedDataStream.ToArray() };
        }

        public BenchmarkScenario CreateTextSearcher()
        {
            if (CompressedData is null)
            {
                throw new InvalidOperationException();
            }
            var compressedDataStream = new MemoryStream(CompressedData, writable: false);
            return this with { TextSearcher = SetUpStrategy.CreateCompressedStreamTextSearcher(compressedDataStream) };
        }
    };

    private BenchmarkScenario _systemGzipScenario = new (new SystemGzipSetUpStrategy(Encoding), null, null);
    private BenchmarkScenario _zlibGzipScenario = new (new  ZlibGzipSetUpStrategy(Encoding), null, null);

    private BenchmarkScenario _zlibGzipParallelScenario = new(
        new ZlibGzipParallelSetUpStrategy(RecoveryPointByteInterval, ParallelZlibGzipOverlapInBytes, Encoding),
        null,
        null);

    [GlobalSetup]
    public void GlobalSetup()
    {
        _systemGzipScenario = _systemGzipScenario.GenerateCompressedData(UncompressedLogSizeInMegabytes);
        _zlibGzipScenario = _zlibGzipScenario.GenerateCompressedData(UncompressedLogSizeInMegabytes);
        _zlibGzipParallelScenario = _zlibGzipParallelScenario.GenerateCompressedData(UncompressedLogSizeInMegabytes);
    }

    [IterationSetup(Target = nameof(SystemGzip_FindAll))]
    public void IterationSetup_SystemGzip()
    {
        _systemGzipScenario = _systemGzipScenario.CreateTextSearcher();
    }

    [IterationSetup(Target = nameof(ZlibGzip_FindAll))]
    public void IterationSetup_ZlibGzip()
    {
        _zlibGzipScenario = _zlibGzipScenario.CreateTextSearcher();
    }

    [IterationSetup(Target = nameof(ZlibGzip_Parallel_FindAll))]
    public void IterationSetup_ZlibGzip_Parallel()
    {
        _zlibGzipParallelScenario = _zlibGzipParallelScenario.CreateTextSearcher();
    }

    [IterationCleanup]
    public void IterationCleanUp()
    {
        _systemGzipScenario.TextSearcher?.Dispose();
        _zlibGzipScenario.TextSearcher?.Dispose();
        _zlibGzipParallelScenario.TextSearcher?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public List<SearchResult> SystemGzip_FindAll() =>
        _systemGzipScenario.TextSearcher?.FindAll(SearchPattern).ToList() ?? [];

    [Benchmark]
    public List<SearchResult> ZlibGzip_FindAll() =>
        _zlibGzipParallelScenario.TextSearcher?.FindAll(SearchPattern).ToList() ?? [];

    [Benchmark]
    public List<SearchResult> ZlibGzip_Parallel_FindAll() =>
        _zlibGzipParallelScenario.TextSearcher?.FindAll(SearchPattern).ToList() ?? [];
}
