using GitHub.Internals;
using System;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public static class Scientist
    {
        // Should be configured once before starting observations.
        // TODO: How can we guide the developer to the pit of success
        public static IResultPublisher ResultPublisher
        {
            get;
            set;
        } = new InMemoryResultPublisher();

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Science<T>(string name, Action<IExperiment<T>> experiment)
        {
            // TODO: Maybe we could automatically generate the name if none is provided using the calling method name. We'd have to 
            // make sure we don't inline this method though.
            var experimentBuilder = new Experiment<T>(name, 1);
            
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
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment)
        {
            return ScienceAsync<T>(name, 1, experiment);
        }

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T>(string name, int concurrentTasks, Action<IExperimentAsync<T>> experiment)
        {
            var experimentBuilder = new Experiment<T>(name, concurrentTasks);
            
            experiment(experimentBuilder);

            return experimentBuilder.Build().Run();
        }
    }
}
