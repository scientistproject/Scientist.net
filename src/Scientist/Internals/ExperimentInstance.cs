using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Internals.Extensions;

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
        internal readonly List<NamedBehavior> Behaviors;
        internal readonly Func<T, TClean> Cleaner;
        internal readonly Func<T, T, bool> Comparator;
        internal readonly Func<Task> BeforeRun;
        internal readonly Func<Task<bool>> Enabled;
        internal readonly Func<Task<bool>> EnableControl;
        internal readonly Func<Task<bool>> RunIf;
        internal readonly IEnumerable<Func<T, T, Task<bool>>> Ignores;
        internal readonly Dictionary<string, dynamic> Contexts;
        internal readonly Action<Operation, Exception> Thrown;
        internal readonly bool ThrowOnMismatches;
        internal readonly IResultPublisher ResultPublisher;
        
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);
        
        public ExperimentInstance(ExperimentSettings<T, TClean> settings)
        {
            Name = settings.Name;

            Behaviors = new List<NamedBehavior>
            {
                new NamedBehavior(ControlExperimentName, settings.Control),
            };
            Behaviors.AddRange(
                settings.Candidates.Select(c => new NamedBehavior(c.Key, c.Value)));

            BeforeRun = settings.BeforeRun;
            Cleaner = settings.Cleaner;
            Comparator = settings.Comparator;
            ConcurrentTasks = settings.ConcurrentTasks;
            Contexts = settings.Contexts;
            Enabled = settings.Enabled;
            EnableControl = settings.EnableControl;
            RunIf = settings.RunIf;
            Ignores = settings.Ignores;
            Thrown = settings.Thrown;
            ThrowOnMismatches = settings.ThrowOnMismatches;
            ResultPublisher = settings.ResultPublisher;
        }

        public async Task<T> Run()
        {
            // Determine if experiments should be run.
            if (!await ShouldExperimentRun().ConfigureAwait(false))
            {
                // Run the control behavior.
                return await Behaviors[0].Behavior().ConfigureAwait(false);
            }

            if (!await ReturnFirstCandidate())
            {
                return await Behaviors[1].Behavior();
            }

            if (BeforeRun != null)
            {
                await BeforeRun().ConfigureAwait(false);
            }

            // Randomize ordering...
            NamedBehavior[] orderedBehaviors;
            lock (_random)
            {
                orderedBehaviors = Behaviors.OrderBy(b => _random.Next()).ToArray();
            }

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

        /// <summary>
        /// Determine whether or not to return the result of the first candidate.
        /// </summary>
        async Task<bool> ReturnFirstCandidate()
        {
            try
            {
                // Only let the experiment run if at least one candidate (> 1 behaviors) is 
                // included.  The control is always included behaviors count.
                return await Enabled() && await RunIfAllows() && await EnableControl();
            }
            catch (Exception ex)
            {
                Thrown(Operation.Enabled, ex);
                return false;
            }
        }

        internal class NamedBehavior
        {
            public NamedBehavior(string name, Func<T> behavior)
                : this(name, () => Task.FromResult(behavior()))
            {
            }

            public NamedBehavior(string name, Func<Task<T>> behavior)
            {
                Behavior = behavior;
                Name = name;
            }

            /// <summary>
            /// Gets the behavior to execute during an experiment.
            /// </summary>
            public Func<Task<T>> Behavior { get; }

            /// <summary>
            /// Gets the name of the behavior.
            /// </summary>
            public string Name { get; }
        }
    }
}