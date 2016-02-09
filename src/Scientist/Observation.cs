using GitHub.Internals;
using System;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Defines an observation of an experiment's execution.
    /// </summary>
    public class Observation
    {
        /// <summary>
        /// Creates a new observation.
        /// </summary>
        /// <param name="name">The name of the experiment that was observed.</param>
        /// <param name="success">Whether the experiment was a success.</param>
        /// <param name="controlDuration">The total duration for the controlled experiment.</param>
        /// <param name="candidateDuration">The total duration for the candidate experiment.</param>
        public Observation(string name, bool success, TimeSpan controlDuration, TimeSpan candidateDuration)
        {
            Name = name;
            Success = success;
            ControlDuration = controlDuration;
            CandidateDuration = candidateDuration;
        }

        /// <summary>
        /// Gets whether the experiment was a success.
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Gets the total duration for the controlled experiment that was executed through
        /// <see cref="IExperiment{T}.Use(Func{T})" /> or <see cref="IExperimentAsync{T}.Use(Func{Task{T}})" />.
        /// </summary>
        public TimeSpan ControlDuration { get; }
        
        /// <summary>
        /// Gets the total duration for the candidate experiment that was executed through
        /// <see cref="IExperiment{T}.Try(Func{T})" /> or <see cref="IExperimentAsync{T}.Try(Func{Task{T}})" />.
        /// </summary>
        public TimeSpan CandidateDuration { get; }
        
        /// <summary>
        /// Gets the name of the experiment that was observed.
        /// </summary>
        public string Name { get; }
     }
}
