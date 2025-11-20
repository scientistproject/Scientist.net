using Github.Ordering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// Provides a base interface for all experiments.
    /// </summary>
    public interface IExperiment
    {
        /// <summary>
        /// Set this flag to throw on experiment mismatches.
        /// 
        /// This causes all science mismatches to throw a <see cref="MismatchException{T}"/>.
        /// This is intended for test environments and should not be enabled in a production
        /// environment.
        /// </summary>
        /// <value>
        /// Whether to throw when the control and candidate mismatch.
        /// </value>
        bool ThrowOnMismatches { get; set; }

        /// <summary>
        /// Defines data to publish with results.
        /// </summary>
        /// <param name="key">The name of the context</param>
        /// <param name="data">The context data</param>
        void AddContext(string key, object data);

        /// <summary>
        /// Defines the exception handler when an exception is thrown during an experiment.
        /// </summary>
        /// <param name="block">The delegate to handle exceptions thrown from an experiment.</param>
        void Thrown(Action<Operation, Exception> block);
    }

    /// <summary>
    /// Provides an interface for defining a synchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperiment<T> : IExperiment
    {
        /// <summary>
        /// Define any expensive setup here before the experiment is run.
        /// </summary>
        void BeforeRun(Action action);

        /// <summary>
        /// Defines the check to run to determine if the experiment should run.
        /// </summary>
        /// <param name="check">The delegate to evaluate.</param>
        void RunIf(Func<bool> check);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<T> candidate);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(string name, Func<T> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<T> control);

        /// <summary>
        /// Defines a custom func used to compare results.
        /// </summary>
        void Compare(Func<T, T, bool> comparison);

        /// <summary>
        /// Defines the check to run to determine if mismatches should be ignored.
        /// </summary>
        /// <param name="block">The delegate to execute.</param>
        void Ignore(Func<T, T, bool> block);

        /// <summary>
        /// Defines the custom ordering to run on the behaviors
        /// </summary>
        /// <param name="ordering">The delgate to execute.</param>
        void UseCustomOrdering(Func<IReadOnlyList<INamedBehavior<T>>, IReadOnlyList<INamedBehavior<T>>> ordering);
    }

    /// <summary>
    /// Provides an interface for defining a synchronous experiment that provides a clean value to publish.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    /// <typeparam name="TClean">The clean result for publishing.</typeparam>
    public interface IExperiment<T, TClean> : IExperiment<T>
    {
        void Clean(Func<T, TClean> cleaner);
    }

    /// <summary>
    /// Provides an interface for defining an asynchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperimentAsync<T> : IExperiment
    {
        /// <summary>
        /// Define any expensive setup here before the experiment is run.
        /// </summary>
        void BeforeRun(Func<Task> action);

        /// <summary>
        /// Defines the check to run to determine if the experiment should run.
        /// </summary>
        /// <param name="check">The delegate to evaluate.</param>
        void RunIf(Func<Task<bool>> block);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<Task<T>> candidate);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(string name, Func<Task<T>> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<Task<T>> control);

        /// <summary>
        /// Defines a func used to compare results.
        /// </summary>
        void Compare(Func<T, T, bool> comparison);

        /// <summary>
        /// Defines the check to run to determine if mismatches should be ignored.
        /// </summary>
        /// <param name="block">The delegate to execute.</param>
        void Ignore(Func<T, T, Task<bool>> block);

        /// <summary>
        /// Defines the custom ordering to run on the behaviors
        /// </summary>
        /// <param name="customOrdering">The delgate to execute.</param>
        void UseCustomOrdering(CustomOrderer<T> customOrdering);
    }

    /// <summary>
    /// Provides an interface for defining an asynchronous experiment that provides a clean value to publish.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    /// <typeparam name="TClean">The clean result for publishing.</typeparam>
    public interface IExperimentAsync<T, TClean> : IExperimentAsync<T>
    {
        void Clean(Func<T, TClean> cleaner);
    }
}