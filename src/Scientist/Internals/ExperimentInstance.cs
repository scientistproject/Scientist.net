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
        internal readonly NamedBehavior Control;
        internal readonly List<NamedBehavior> Candidates = new List<NamedBehavior>();
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
        internal readonly bool EnsureControlRunsFirst;

        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        public ExperimentInstance(ExperimentSettings<T, TClean> settings)
        {
            Name = settings.Name;
            Control = new NamedBehavior(ControlExperimentName, settings.Control);
            Candidates.AddRange(
                settings.Candidates.Select(c => new NamedBehavior(c.Key, c.Value)));

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
            EnsureControlRunsFirst = settings.EnsureControlRunsFirst;
        }

        public async Task<T> Run()
        {
            // Determine if experiments should be run.
            if (!await ShouldExperimentRun().ConfigureAwait(false))
            {
                // Run the control behavior.
                return await Control.Behavior().ConfigureAwait(false);
            }

            if (BeforeRun != null)
            {
                await BeforeRun().ConfigureAwait(false);
            }


            var behaviors = new NamedBehavior[0];
            if (EnsureControlRunsFirst)
            {

                behaviors = RandomiseBehavioursOrder(Candidates);
                behaviors = new[] { Control }.Concat(behaviors).ToArray();
            }
            else
            {
                Candidates.Add(Control);
                behaviors = RandomiseBehavioursOrder(Candidates);
            }


            // Break tasks into batches of "ConcurrentTasks" size
            var observations = new List<Observation<T, TClean>>();
            foreach (var behaviorsChunk in behaviors.Chunk(ConcurrentTasks))
            {
                // Run batch of behaviors simultaneously
                var tasks = behaviorsChunk.Select(b =>
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

        private NamedBehavior[] RandomiseBehavioursOrder(List<NamedBehavior> behaviors)
        {
            lock (_random)
            {
                return behaviors.OrderBy(b => _random.Next()).ToArray();
            }
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
                // Only let the experiment run if at least one candidate (>= 1 behaviors)
                return Candidates.Count >= 1 && await Enabled().ConfigureAwait(false) && await RunIfAllows().ConfigureAwait(false);
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