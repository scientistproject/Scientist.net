using System;
using System.Threading.Tasks;
using GitHub.Internals;
using NullGuard;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public static class Scientist
    {
        // TODO: Evaluate the distribution of Random and whether it's good enough.
        static readonly Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);                                

        // Should be configured once before starting observations.
        // TODO: How can we guide the developer to the pit of success
        public static IObservationPublisher ObservationPublisher
        {
            get;
            set;
        } = new InMemoryObservationPublisher();

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        [return: AllowNull]
        public static T Science<T>(string name, Action<IExperiment<T>> experiment)
        {
            // TODO: Maybe we could automatically generate the name if none is provided using the calling method name. We'd have to 
            // make sure we don't inline this method though.
            var experimentBuilder = new Experiment<T>(name);
            
            experiment(experimentBuilder);

            return experimentBuilder.Build().Run().Result;
        }

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        [return: AllowNull]
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment)
        {
            var experimentBuilder = new Experiment<T>(name);
            
            experiment(experimentBuilder);

            return experimentBuilder.Build().Run();
        }
    }
}
