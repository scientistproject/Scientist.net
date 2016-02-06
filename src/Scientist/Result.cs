using System.Collections.Generic;
using System.Linq;

namespace GitHub
{
    /// <summary>
    /// Provides an observed result.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Gets all of the candidate observations.
        /// </summary>
        IReadOnlyList<IObservation> Candidates { get; }

        /// <summary>
        /// Gets the controlled observation.
        /// </summary>
        IObservation Control { get; }

        /// <summary>
        /// Gets the name of the experiment.
        /// </summary>
        string ExperimentName { get; }

        /// <summary>
        /// Gets whether the candidate observations matched the controlled observation.
        /// </summary>
        bool Matched { get; }

        /// <summary>
        /// Gets whether any of the candidate observations did not match the controlled observation.
        /// </summary>
        bool Mismatched { get; }

        /// <summary>
        /// Gets all of the observations that did not match the controlled observation.
        /// </summary>
        IReadOnlyList<IObservation> MismatchedObservations { get; }

        /// <summary>
        /// Gets all of the observations.
        /// </summary>
        IReadOnlyList<IObservation> Observations { get; }
    }

    public class Result<T> : IResult
    {
        public Result(string experimentName, IEnumerable<Observation<T>> observations, Observation<T> control)
        {
            Candidates = observations.Where(o => o != control).ToList();
            Control = control;
            ExperimentName = experimentName;
            Observations = observations.ToList();

            MismatchedObservations = Candidates.Where(o => o != Control).ToList();
            
            // TODO Implement ignored observations.
        }

        /// <summary>
        /// Gets all of the candidate observations.
        /// </summary>
        public IReadOnlyList<IObservation> Candidates { get; }

        /// <summary>
        /// Gets the controlled observation.
        /// </summary>
        public IObservation Control { get; }

        /// <summary>
        /// Gets the name of the experiment.
        /// </summary>
        public string ExperimentName { get; }

        /// <summary>
        /// Gets whether the candidate observations matched the controlled observation.
        /// </summary>
        public bool Matched => !MismatchedObservations.Any();

        /// <summary>
        /// Gets whether any of the candidate observations did not match the controlled observation.
        /// </summary>
        public bool Mismatched => MismatchedObservations.Any();

        /// <summary>
        /// Gets all of the observations that did not match the controlled observation.
        /// </summary>
        public IReadOnlyList<IObservation> MismatchedObservations { get; }

        /// <summary>
        /// Gets all of the observations.
        /// </summary>
        public IReadOnlyList<IObservation> Observations { get; }
    }
}
