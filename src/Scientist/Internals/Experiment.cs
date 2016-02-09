using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Scientist.Internals;

namespace GitHub.Internals
{
    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="T">The return type of the experiment</typeparam>
    internal class ExperimentInstance<T>
    {
        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        readonly Func<Task<T>> _control;
        readonly Func<Task<T>> _candidate;

        readonly ExperimentResultComparer<T> _experimentResultComparer;
        

        readonly string _name;

        public ExperimentInstance(string name, Func<T> control, Func<T> candidate)
        {
            _name = name;
            _control = () => Task.FromResult(control());
            _candidate = () => Task.FromResult(candidate());
            //_experimentResultComparer = experimentResultComparer;
        }
        public ExperimentInstance(string name, Func<T> control, Func<T> candidate, ExperimentResultComparer<T> experimentResultComparer)
        {
            _name = name;
            _control = () => Task.FromResult(control());
            _candidate = () => Task.FromResult(candidate());
            _experimentResultComparer = experimentResultComparer;
        }

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T>> candidate, ExperimentResultComparer<T> experimentResultComparer)
        {
            _name = name;
            _control = control;
            _candidate = candidate;
            _experimentResultComparer = experimentResultComparer;
        }
  

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T>> candidate)
        {
            _name = name;
            _control = control;
            _candidate = candidate;
        }

       


        public async Task<T> Run()
        {
            // Randomize ordering...
            var runControlFirst = _random.Next(0, 2) == 0;
            ExperimentResult controlResult;
            ExperimentResult candidateResult;


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

            bool success = _experimentResultComparer.Equals(controlResult, candidateResult);

            var observation = new Observation(_name, success, controlResult.Duration, candidateResult.Duration);

            // TODO: Make this Fire and forget so we don't have to wait for this
            // to complete before we return a result
            await Scientist.ObservationPublisher.Publish(observation);

            if (controlResult.ThrownException != null) throw controlResult.ThrownException;
            return controlResult.Result;
        }

        

        static async Task<ExperimentResult> Run(Func<Task<T>> experimentCase)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                // TODO: Refactor this into helper function?  
                var result = await experimentCase();
                sw.Stop();

                return new ExperimentResult(result, new TimeSpan(sw.ElapsedTicks));
            }
            catch (Exception e)
            {
                sw.Stop();
                return new ExperimentResult(e, new TimeSpan(sw.ElapsedTicks));
            }
        }

        internal class ExperimentResult
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
