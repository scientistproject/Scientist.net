using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Internals;

namespace GitHub
{
    /// <summary>
    /// Defines extension methods for the <see cref="IScientist"/> interface
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ScientistExtensions
    {
        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="scientist">The scientist implementation to use</param>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Experiment<T>(this IScientist scientist, string name, Action<IExperiment<T>> experiment) =>
            scientist.Experiment<T, T>(name, e => experiment(e));

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="scientist">The scientist implementation to use</param>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ExperimentAsync<T>(this IScientist scientist, string name, Action<IExperimentAsync<T>> experiment) =>
            scientist.ExperimentAsync(name, 1, experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="scientist">The scientist implementation to use</param>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ExperimentAsync<T>(this IScientist scientist, string name, int concurrentTasks, Action<IExperimentAsync<T>> experiment) =>
            scientist.ExperimentAsync<T, T>(name, concurrentTasks, e => experiment(e));

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="scientist">The scientist implementation to use</param>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ExperimentAsync<T, TClean>(this IScientist scientist, string name, Action<IExperimentAsync<T, TClean>> experiment) =>
            scientist.ExperimentAsync(name, 1, experiment);
    }
}
