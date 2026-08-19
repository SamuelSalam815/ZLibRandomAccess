using BenchmarkDotNet.Configs;

namespace Benchmarking;

using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
        var summary = BenchmarkRunner.Run<BenchmarkingRegexInGzipStream>();
    }
}
