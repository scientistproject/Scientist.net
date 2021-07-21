using System.Collections.Generic;
using System.Linq;
using GitHub.Internals;

namespace GitHub
{
    public class Result<T, TClean>
    {
        internal Result(ExperimentInstance<T, TClean> experiment, IEnumerable<Observation<T, TClean>> observations, Observation<T, TClean> control, Dictionary<string, dynamic> contexts)
        {
            Candidates = observations.Where(o => o != control).ToList();
            Control = control;
            ExperimentName = experiment.Name;
            Observations = observations.ToList();
            Contexts = contexts;

            var mismatchedObservations = Candidates.Where(o => !o.EquivalentTo(Control, experiment.Comparator)).ToList();

            IgnoredObservations = mismatchedObservations.Where(m => experiment.IgnoreMismatchedObservation(control, m).Result).ToList();

            MismatchedObservations = mismatchedObservations.Except(IgnoredObservations).ToList();
        }

        /// <summary>
        /// Gets all of the candidate observations.
        /// </summary>
        public IReadOnlyList<Observation<T, TClean>> Candidates { get; }

        /// <summary>
        /// Gets the controlled observation.
        /// </summary>
        public Observation<T, TClean> Control { get; }

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
        public IReadOnlyList<Observation<T, TClean>> MismatchedObservations { get; }

        /// <summary>
        /// Gets all of the observations.
        /// </summary>
        public IReadOnlyList<Observation<T, TClean>> Observations { get; }

        /// <summary>
        /// Gets all of the mismatched observations whos values where ignored.
        /// </summary>
        public IReadOnlyList<Observation<T, TClean>> IgnoredObservations { get; }

        /// <summary>
        /// Gets the context data supplied to the experiment.
        /// </summary>
        public IReadOnlyDictionary<string, dynamic> Contexts { get; }
    }
}
