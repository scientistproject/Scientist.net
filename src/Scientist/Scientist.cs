using System;
using GitHub.Internals;
using System.Threading.Tasks;
using NullGuard;

namespace GitHub
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public static class Scientist
    {
        // TODO: Evaluate the distribution of Random and whether it's good enough.
        static readonly Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);                                

        // Should be configured once before starting measurements.
        public static IMeasurementPublisher MeasurementPublisher
        {
            get;
            set;
        } = new InMemoryPublisher();

        [return: AllowNull]
        public static T Science<T>(string name, Action<IExperiment<T>> experiment)
        {
            var experimentBuilder = new Experiment<T>(name);
            
            experiment(experimentBuilder);

            return experimentBuilder.Build().Run().Result;
        }

        [return: AllowNull]
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment)
        {
            var experimentBuilder = new Experiment<T>(name);
            
            experiment(experimentBuilder);

            return experimentBuilder.Build().Run();
        }
    }
}
