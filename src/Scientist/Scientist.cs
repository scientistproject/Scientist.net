using GitHub.Internals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public static class Scientist
    {
        static Func<Task<bool>> _enabled = () => Task.FromResult(true);
        static readonly ConcurrentSet<Task> _publishingTasks = new ConcurrentSet<Task>();

        // Should be configured once before starting observations.
        // TODO: How can we guide the developer to the pit of success
        public static IResultPublisher ResultPublisher
        {
            get;
            set;
        } = new InMemoryResultPublisher();
        
        static Experiment<T, TClean> Build<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment)
        {
            // TODO: Maybe we could automatically generate the name if none is provided using the calling method name. We'd have to 
            // make sure we don't inline this method though.
            var experimentBuilder = new Experiment<T, TClean>(name, _publishingTasks, _enabled);

            experiment(experimentBuilder);

            return experimentBuilder;
        }

        static Experiment<T, TClean> Build<T, TClean>(string name, Action<IExperimentAsync<T, TClean>> experiment)
        {
            var builder = new Experiment<T, TClean>(name, _publishingTasks, _enabled);

            experiment(builder);

            return builder;
        }

        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <param name="enabled">A delegate returning if an experiment should run.</param>
        public static void Enabled(Func<bool> enabled) => Enabled(() => Task.FromResult(enabled()));

        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <param name="enabled">A delegate returning an asynchronous task determining if an experiment should run.</param>
        public static void Enabled(Func<Task<bool>> enabled) => _enabled = enabled;

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Science<T>(string name, Action<IExperiment<T>> experiment)
        {
            var builder = Build<T, T>(name, experiment);
            builder.Clean(value => value);
            return builder.Build().Run().Result;
        }

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Science<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment) =>
            Build(name, experiment).Build().Run().Result;

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment)
        {
            var builder = Build<T, T>(name, experiment);
            builder.Clean(value => value);
            return builder.Build().Run();
        }

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T, TClean>(string name, Action<IExperimentAsync<T, TClean>> experiment) =>
            Build(name, experiment).Build().Run();

        /// <summary>
        /// Creates a new task that is completed when all remaining publishing tasks have completed.
        /// </summary>
        public static Task WhenPublished() => Task.WhenAll(_publishingTasks.Items);
    }
}
