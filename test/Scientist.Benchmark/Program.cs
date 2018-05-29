using BenchmarkDotNet.Running;

namespace GitHub
{
    static class Program
    {
        static void Main() => BenchmarkRunner.Run<ScientistBenchmarks>();
    }
}
