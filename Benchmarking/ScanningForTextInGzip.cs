using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Benchmarking;

public class ScanningForTextInGzip
{
    [ParamsSource(nameof(ValuesForUncompressedLogMegabyteCount))]
    public int UncompressedLogMegabyteCount { get; set; }

    public IEnumerable<int> ValuesForUncompressedLogMegabyteCount => Enumerable
        .Sequence(2, 6, 1)
        .Select(power => (int)Math.Pow(2, power) * 1024 * 1024);

    public static readonly Regex SearchPattern = new(
        @"after completing \d+ encounters and performing [23456789]\d* limit breaks");

    // private MemoryStream? _compressedData;

    private static void GenerateLogFileFor(string variant)
    {

    }


}
