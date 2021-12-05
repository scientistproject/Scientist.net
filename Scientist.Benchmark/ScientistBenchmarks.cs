using BenchmarkDotNet.Attributes;

namespace Scientist.Benchmark
{
    [MemoryDiagnoser]
    public class ScientistBenchmarks
    {
        const string Name = "SynchronousPass";
        private static int FortyTwo() => 42;
        readonly Scientist _science;

        public ScientistBenchmarks()
        {
            _science = new Scientist(new Internals.InMemoryResultPublisher());
        }

        [Benchmark(Baseline = true)]
        public int Static()
        {
            return Scientist.Science<int>(Name, (experiment) =>
            {
                experiment.Use(FortyTwo);
                experiment.Try(FortyTwo);
            });
        }

        [Benchmark(Baseline = false)]
        public int Instance()
        {
            return _science.Experiment<int>(Name, (experiment) =>
            {
                experiment.Use(FortyTwo);
                experiment.Try(FortyTwo);
            });
        }

    }
}
