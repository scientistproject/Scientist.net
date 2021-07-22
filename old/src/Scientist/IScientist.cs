using System;
using System.Threading.Tasks;
using GitHub.Internals;

namespace GitHub
{
    /// <summary>
    /// An interface for carefully refactoring critical paths
    /// </summary>
    public interface IScientist
    {
        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        T Experiment<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        Task<T> ExperimentAsync<T, TClean>(string name, int concurrentTasks, Action<IExperimentAsync<T, TClean>> experiment);
    }
}
