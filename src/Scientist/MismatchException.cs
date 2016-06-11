using GitHub.Internals;
using System;

namespace GitHub
{
    /// <summary>
    /// A mismatch thrown when <see cref="IExperiment{T}.ThrowOnMismatches"/>
    /// or <see cref="IExperimentAsync{T}.ThrowOnMismatches"/> is enabled.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    /// <typeparam name="TClean">The clean value for the experiment.</typeparam>
    public class MismatchException<T, TClean> : Exception
    {
        public MismatchException(string name, Result<T, TClean> result)
            : base($"Experiment '{name}' observations mismatched")
        {
            Name = name;
            Result = result;
        }

        /// <summary>
        /// Gets the name of the experiment.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the result containing a mismatch.
        /// </summary>
        public Result<T, TClean> Result { get; }
    }
}
