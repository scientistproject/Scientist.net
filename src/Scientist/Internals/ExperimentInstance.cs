using Github.Ordering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="T">The return type of the experiment</typeparam>
    /// <typeparam name="TClean">The cleaned type of the experiment</typeparam>
    internal class ExperimentInstance<T, TClean>
    {
        internal const string ControlExperimentName = "control";

        internal readonly string Name;
        internal readonly int ConcurrentTasks;
        internal readonly List<NamedBehavior<T>> Behaviors;
        internal readonly Func<T, TClean> Cleaner;
        internal readonly Func<T, T, bool> Comparator;
        internal readonly Func<Task> BeforeRun;
        internal readonly Func<Task<bool>> Enabled;
        internal readonly Func<Task<bool>> RunIf;
        internal readonly IEnumerable<Func<T, T, Task<bool>>> Ignores;
        internal readonly Dictionary<string, dynamic> Contexts;
        internal readonly Action<Operation, Exception> Thrown;
        internal readonly bool ThrowOnMismatches;
        internal readonly IResultPublisher ResultPublisher;
        internal readonly CustomOrderer<T> CustomOrderer;

        public ExperimentInstance(ExperimentSettings<T, TClean> settings)
        {
            Name = settings.Name;

            Behaviors = new List<NamedBehavior<T>>
            {
                new NamedBehavior<T>(ControlExperimentName, settings.Control),
            };
            Behaviors.AddRange(
                settings.Candidates.Select(c => new NamedBehavior<T>(c.Key, c.Value)));

            BeforeRun = settings.BeforeRun;
            Cleaner = settings.Cleaner;
            Comparator = settings.Comparator;
            ConcurrentTasks = settings.ConcurrentTasks;
            Contexts = settings.Contexts;
            Enabled = settings.Enabled;
            RunIf = settings.RunIf;
            Ignores = settings.Ignores;
            Thrown = settings.Thrown;
            ThrowOnMismatches = settings.ThrowOnMismatches;
            ResultPublisher = settings.ResultPublisher;
            CustomOrderer = settings.CustomOrderer;
        }

        public async Task<T> Run()
        {
            // Determine if experiments should be run.
            if (!await ShouldExperimentRun().ConfigureAwait(false))
            {
                // Run the control behavior.
                return await Behaviors[0].Behavior().ConfigureAwait(false);
            }

            if (BeforeRun != null)
            {
                await BeforeRun().ConfigureAwait(false);
            }

            var orderedBehaviors = await CustomOrderer(Behaviors).ConfigureAwait(false);

            // Break tasks into batches of "ConcurrentTasks" size
            var observations = new List<Observation<T, TClean>>();
            foreach (var behaviors in orderedBehaviors.Chunk(ConcurrentTasks))
            {
                // Run batch of behaviors simultaneously
                var tasks = behaviors.Select(b =>
                {
                    return Observation<T, TClean>.New(
                        b.Name,
                        b.Behavior,
                        Comparator,
                        Thrown,
                        Cleaner);
                });

                // Collect the observations
                observations.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }

            var controlObservation = observations.FirstOrDefault(o => o.Name == ControlExperimentName);

            var result = new Result<T, TClean>(this, observations, controlObservation, Contexts);

            try
            {
                await ResultPublisher.Publish(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Thrown(Operation.Publish, ex);
            }

            if (ThrowOnMismatches && result.Mismatched)
            {
                throw new MismatchException<T, TClean>(Name, result);
            }

            if (controlObservation.Thrown) throw controlObservation.Exception;
            return controlObservation.Value;
        }

        /// <summary>
        /// Does <see cref="RunIf"/> allow the experiment to run?
        /// </summary>
        async Task<bool> RunIfAllows()
        {
            try { return await RunIf().ConfigureAwait(false); }
            catch (Exception ex)
            {
                Thrown(Operation.RunIf, ex);
                return false;
            }
        }

        public async Task<bool> IgnoreMismatchedObservation(Observation<T, TClean> control, Observation<T, TClean> candidate)
        {
            if (!Ignores.Any())
            {
                return false;
            }

            try
            {
                //TODO: Does this really need to be async? We could run sync and return on first true
                var results = await Task.WhenAll(Ignores.Select(i => i(control.Value, candidate.Value))).ConfigureAwait(false);

                return results.Any(i => i);
            }
            catch (Exception ex)
            {
                Thrown(Operation.Ignore, ex);
                return false;
            }
        }

        /// <summary>
        /// Determine whether or not the experiment should run.
        /// </summary>
        async Task<bool> ShouldExperimentRun()
        {
            try
            {
                // Only let the experiment run if at least one candidate (> 1 behaviors) is 
                // included.  The control is always included behaviors count.
                return Behaviors.Count > 1 && await Enabled().ConfigureAwait(false) && await RunIfAllows().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Thrown(Operation.Enabled, ex);
                return false;
            }
        }
    }
}