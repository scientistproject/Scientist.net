using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="T">The return type of the experiment</typeparam>
    internal class ExperimentInstance<T> : ExperimentInstance<T, T>
    {
        public ExperimentInstance(string name, Func<T> control, Func<T> candidate)
            : base(name, control, candidate)
        {

        }

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T>> candidate)
            : base(name, control, candidate)
        {
            
        }
    }

    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="TControl">The control type of the experiment</typeparam>
    /// <typeparam name="TCandidate">The candidate type of the experiment</typeparam>
    internal class ExperimentInstance<TControl, TCandidate>
    {
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        readonly Func<Task<TControl>> _control;
        readonly Func<Task<TCandidate>> _candidate;
        readonly string _name;

        public ExperimentInstance(string name, Func<TControl> control, Func<TCandidate> candidate)
        {
            _name = name;
            _control = () => Task.FromResult(control());
            _candidate = () => Task.FromResult(candidate());
        }

        public ExperimentInstance(string name, Func<Task<TControl>> control, Func<Task<TCandidate>> candidate)
        {
            _name = name;
            _control = control;
            _candidate = candidate;
        }

        public async Task<TControl> Run()
        {
            // Randomize ordering...
            var runControlFirst = _random.Next(0, 2) == 0;
            ExperimentResult<TControl> controlResult;
            ExperimentResult<TCandidate> candidateResult;

            if (runControlFirst)
            {
                controlResult = await Run(_control);
                candidateResult = await Run(_candidate);
            }
            else
            {
                candidateResult = await Run(_candidate);
                controlResult = await Run(_control);
            }

            // TODO: We need to compare that thrown exceptions are equivalent too https://github.com/github/scientist/blob/master/lib/scientist/observation.rb#L76
            // TODO: We're going to have to be a bit more sophisticated about this.
            bool success =
                controlResult.Result == null && candidateResult.Result == null
                || controlResult.Result != null && controlResult.Result.Equals(candidateResult.Result)
                || controlResult.Result == null && candidateResult.Result != null;

            // TODO: Get that duration!
            var observation = new Observation(_name, success, controlResult.Duration, candidateResult.Duration);

            // TODO: Make this Fire and forget so we don't have to wait for this
            // to complete before we return a result
            await Scientist.ObservationPublisher.Publish(observation);

            if (controlResult.ThrownException != null) throw controlResult.ThrownException;
            return controlResult.Result;
        }

        static async Task<ExperimentResult<TResult>> Run<TResult>(Func<Task<TResult>> experimentCase)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                // TODO: Refactor this into helper function?  
                var result = await experimentCase();
                sw.Stop();

                return new ExperimentResult<TResult>(result, new TimeSpan(sw.ElapsedTicks));
            }
            catch (Exception e)
            {
                sw.Stop();
                return new ExperimentResult<TResult>(e,  new TimeSpan(sw.ElapsedTicks));
            }
        }

        class ExperimentResult<T>
        {
            public ExperimentResult(T result, TimeSpan duration)
            {
                Result = result;
                Duration = duration;
            }

            public ExperimentResult(Exception exception, TimeSpan duration)
            {
                ThrownException = exception;
                Duration = duration;
            }

            public T Result { get; }

            public Exception ThrownException { get; }

            public TimeSpan Duration { get; }
        }
    }
}