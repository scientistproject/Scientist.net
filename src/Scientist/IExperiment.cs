using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// Provides an interface for defining a synchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperiment<TControl, TCandidate>
    {
        void Try(Func<TCandidate> candidate);

        void Use(Func<TControl> control);
    }

    public interface IExperimentAsync<TControl, TCandidate>
    {
        void Try(Func<Task<TCandidate>> candidate);

        void Use(Func<Task<TControl>> control);
    }

    public interface IExperiment<T>
    {
        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<T> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<T> control);
    }

    /// <summary>
    /// Provides an interface for defining an asynchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperimentAsync<T>
    {
        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<Task<T>> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<Task<T>> control);
    }
}