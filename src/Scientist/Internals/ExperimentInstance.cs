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
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        readonly Dictionary<string, Func<Task<T>>> _behaviors;
        readonly string _name;
        readonly Func<Task<bool>> _runIf;

        public ExperimentInstance(string name, Func<T> control, Func<T> candidate, Func<bool> runIf)
            : this(name, () => Task.FromResult(control()), () => Task.FromResult(candidate()), () => Task.FromResult(runIf()))
        {
        }

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T>> candidate, Func<Task<bool>> runIf)
        {
            _name = name;
            _behaviors = new Dictionary<string, Func<Task<T>>>
            {
                { "control", control },
                { "candidate", candidate }
            };
            _runIf = runIf;
        }

        public async Task<T> Run()
        {
            const string name = "control";

            // Determine if experiments should be run.
            if (!await _runIf())
            {
                return await _behaviors[name]();
            }

            // Randomize ordering...
            var observations = new List<Observation<T>>();
            foreach (string key in _behaviors.Keys.OrderBy(k => _random.Next()))
            {
                observations.Add(await Observation<T>.New(key, _behaviors[key]));
            }

            Observation<T> controlObservation = observations.FirstOrDefault(o => o.Name == name);
            Result<T> result = new Result<T>(_name, observations, controlObservation);

            // TODO: Make this Fire and forget so we don't have to wait for this
            // to complete before we return a result
            await Scientist.ObservationPublisher.Publish(result);

            if (controlObservation.Thrown) throw controlObservation.Exception;
            return controlObservation.Value;
        }
    }
}