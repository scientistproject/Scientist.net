using BenchmarkDotNet.Running;

namespace Scientist.Benchmark
{
    static class Program
    {
        static void Main() => BenchmarkRunner.Run<ScientistBenchmarks>();
    }
}