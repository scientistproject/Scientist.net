using System.Collections.Generic;
using System.Linq;
using GitHub.Internals;

namespace GitHub
{
    public class Result<T>
    {
        internal Result(ExperimentInstance<T> experiment, IEnumerable<Observation<T>> observations, Observation<T> control)
        {
            Candidates = observations.Where(o => o != control).ToList();
            Control = control;
            ExperimentName = experiment.Name;
            Observations = observations.ToList();

            MismatchedObservations = Candidates.Where(o => !o.EquivalentTo(Control, experiment.Comparator)).ToList();

            IgnoredObservations = MismatchedObservations.Where(m => experiment.IgnoreMismatchedObservation(control, m).Result).ToList();
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
        public bool Matched => !MismatchedObservations.Any() || IgnoredObservations.Any();

        /// <summary>
        /// Gets whether any of the candidate observations did not match the controlled observation.
        /// </summary>
        public bool Mismatched => !Matched;

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
