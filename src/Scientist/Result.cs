using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub
{
    public class Result<T>
    {
        public Result(string experimentName, IEnumerable<Observation<T>> observations, Observation<T> control, Func<T, T, bool> comparator, bool ignored)
        {
            Candidates = observations.Where(o => o != control).ToList();
            Control = control;
            ExperimentName = experimentName;
            Observations = observations.ToList();

            MismatchedObservations = Candidates.Where(o => !ignored && !o.EquivalentTo(Control, comparator)).ToList();

            IgnoredObservations = Candidates.Except(MismatchedObservations).ToList();
        }

        /// <summary>
        /// Gets all of the candidate observations.
        /// </summary>
        public IReadOnlyList<Observation<T>> Candidates { get; }

        /// <summary>
        /// Gets the controlled observation.
        /// </summary>
        public Observation<T> Control { get; }

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
        public IReadOnlyList<Observation<T>> MismatchedObservations { get; }

        /// <summary>
        /// Gets all of the observations.
        /// </summary>
        public IReadOnlyList<Observation<T>> Observations { get; }

        /// <summary>
        /// Gets all of the mismatched observations whos values where ignored.
        /// </summary>
        public IReadOnlyList<Observation<T>> IgnoredObservations { get; }
    }
}
