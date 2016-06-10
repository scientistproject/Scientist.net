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
    internal class ExperimentInstance<T>
    {
        internal const string ControlExperimentName = "control";

        internal readonly string Name;
        internal readonly int ConcurrentTasks;
        internal readonly List<NamedBehavior> Behaviors;
        internal readonly Func<T, T, bool> Comparator;
        internal readonly Func<Task> BeforeRun;
        internal readonly Func<Task<bool>> RunIf;
        internal readonly IEnumerable<Func<T, T, Task<bool>>> Ignores;
        internal readonly Dictionary<string, dynamic> Contexts;
        internal readonly Action<Operation, Exception> Thrown;
        internal readonly bool ThrowOnMismatches;
        
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);
        
        public ExperimentInstance(ExperimentSettings<T> settings)
        {
            Name = settings.Name;

            Behaviors = new List<NamedBehavior>
            {
                new NamedBehavior(ControlExperimentName, settings.Control),
            };
            Behaviors.AddRange(
                settings.Candidates.Select(c => new NamedBehavior(c.Key, c.Value)));

            BeforeRun = settings.BeforeRun;
            Comparator = settings.Comparator;
            ConcurrentTasks = settings.ConcurrentTasks;
            Contexts = settings.Contexts;
            RunIf = settings.RunIf;
            Ignores = settings.Ignores;
            Thrown = settings.Thrown;
            ThrowOnMismatches = settings.ThrowOnMismatches;
        }

        public async Task<T> Run()
        {
            // Determine if experiments should be run.
            if (!await ShouldExperimentRun())
            {
                // Run the control behavior.
                return await Behaviors[0].Behavior();
            }

            if (BeforeRun != null)
            {
                await BeforeRun();
            }

            // Randomize ordering...
            NamedBehavior[] orderedBehaviors;
            lock (_random)
            {
                orderedBehaviors = Behaviors.OrderBy(b => _random.Next()).ToArray();
            }

            var observations = new List<Observation<T>>();
            foreach (var behavior in orderedBehaviors)
            {
                observations.Add(await Observation<T>.New(behavior.Name, behavior.Behavior, Comparator, Thrown));
            }

            var controlObservation = observations.FirstOrDefault(o => o.Name == ControlExperimentName);
            
            var result = new Result<T>(this, observations, controlObservation, Contexts);

            try
            {
                // TODO: Make this Fire and forget so we don't have to wait for this
                // to complete before we return a result
                await Scientist.ResultPublisher.Publish(result);
            }
            catch (Exception ex)
            {
                Thrown(Operation.Publish, ex);
            }

            if (ThrowOnMismatches && result.Mismatched)
            {
                throw new MismatchException<T>(Name, result);
            }

            if (controlObservation.Thrown) throw controlObservation.Exception;
            return controlObservation.Value;
        }
        
        /// <summary>
        /// Does <see cref="RunIf"/> allow the experiment to run?
        /// </summary>
        async Task<bool> RunIfAllows()
        {
            try { return await RunIf(); }
            catch (Exception ex)
            {
                Thrown(Operation.RunIf, ex);
                return false;
            }
        }

        public async Task<bool> IgnoreMismatchedObservation(Observation<T> control, Observation<T> candidate)
        {
            if (!Ignores.Any())
            {
                return false;
            }

            try
            {
                //TODO: Does this really need to be async? We could run sync and return on first true
                var results = await Task.WhenAll(Ignores.Select(i => i(control.Value, candidate.Value)));

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
                // TODO Implement Enabled here.

                // Only let the experiment run if at least one candidate (> 1 behaviors) is 
                // included.  The control is always included behaviors count.
                return Behaviors.Count > 1 && await RunIfAllows();
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