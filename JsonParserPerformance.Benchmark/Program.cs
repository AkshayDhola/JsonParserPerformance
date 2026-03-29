using BenchmarkDotNet.Running;
namespace JsonParserPerformance.Benchmark;

internal static class Program
{
    static void Main()
    {
        _ = BenchmarkRunner.Run<ParserBenchmarks>();
    }
}
