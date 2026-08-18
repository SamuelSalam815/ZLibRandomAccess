using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;

namespace Benchmarking;

using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<RegexInGzipStream>();
    }
}
