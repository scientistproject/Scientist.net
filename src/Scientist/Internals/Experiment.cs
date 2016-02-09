using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="T">The return type of the experiment</typeparam>
    internal class ExperimentInstance<T, T1>
    {
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);
        static Func<T, T1, bool> _defaultComparer = (controlResult, candidateResult) => (controlResult == null && candidateResult == null)
                                                                                        || (controlResult != null && controlResult.Equals(candidateResult))
                                                                                        || (controlResult == null && candidateResult != null);

        readonly Func<Task<T>> _control;
        readonly Func<Task<T1>> _candidate;
        readonly Func<T, T1, bool> _comparer;
        readonly string _name;

        public ExperimentInstance(string name, Func<T> control, Func<T1> candidate, Func<T, T1, bool> comparer)
        {
            _name = name;
            _control = () => Task.FromResult(control());
            _candidate = () => Task.FromResult(candidate());

            if (typeof(T) != typeof(T1) && comparer == null)
            {
                throw new NoComparerException(typeof(T), typeof(T1));
            }

            _comparer = comparer ?? _defaultComparer;
        }

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T1>> candidate, Func<T, T1, bool> comparer)
        {
            _name = name;
            _control = control;
            _candidate = candidate;

            if (typeof(T) != typeof(T1) && comparer == null)
            {
                throw new NoComparerException(typeof(T), typeof(T1));
            }

            _comparer = comparer ?? _defaultComparer;
        }

        public async Task<T> Run()
        {
            // Randomize ordering...
            var runControlFirst = _random.Next(0, 2) == 0;
            ExperimentResult<T> controlResult;
            ExperimentResult<T1> candidateResult;

            if (runControlFirst)
            {
                controlResult = await RunCase(_control);
                candidateResult = await RunCase(_candidate);
            }
            else
            {
                candidateResult = await RunCase(_candidate);
                controlResult = await RunCase(_control);
            }

            // TODO: We need to compare that thrown exceptions are equivalent too https://github.com/github/scientist/blob/master/lib/scientist/observation.rb#L76
            // TODO: We're going to have to be a bit more sophisticated about this.
            var success = _comparer(controlResult.Result, candidateResult.Result);

            // TODO: Get that duration!
            var observation = new Observation(_name, success, controlResult.Duration, candidateResult.Duration);

            // TODO: Make this Fire and forget so we don't have to wait for this
            // to complete before we return a result
            await Scientist.ObservationPublisher.Publish(observation);

            if (controlResult.ThrownException != null) throw controlResult.ThrownException;
            return controlResult.Result;
        }

        static async Task<ExperimentResult<TC>> RunCase<TC>(Func<Task<TC>> experimentCase)
        {
            try
            {
                // TODO: Refactor this into helper function?  
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var result = await experimentCase();
                sw.Stop();

                return new ExperimentResult<TC>(result, new TimeSpan(sw.ElapsedTicks));
            }
            catch (Exception e)
            {
                return new ExperimentResult<TC>(e, TimeSpan.Zero);
            }
        }

        class ExperimentResult<TR>
        {
            public ExperimentResult(TR result, TimeSpan duration)
            {
                Result = result;
                Duration = duration;
            }

            public ExperimentResult(Exception exception, TimeSpan duration)
            {
                ThrownException = exception;
                Duration = duration;
            }

            public TR Result { get; }

            public Exception ThrownException { get; }

            public TimeSpan Duration { get; }
        }
    }
}