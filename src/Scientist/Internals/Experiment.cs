using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T> : IExperiment<T>, IExperimentAsync<T>
    {
        internal const string CandidateExperimentName = "candidate";

        readonly static Func<Task<bool>> _alwaysRun = () => Task.FromResult(true);

        string _name;
        Func<Task<T>> _control;

        //TODO: Do we need a thread safe dictionary?
        readonly Dictionary<string, Func<Task<T>>> _candidates;
        Func<T, T, bool> _comparison = DefaultComparison;
        Func<Task> _beforeRun;
        Func<Task<bool>> _runIf = _alwaysRun;

        public Experiment(string name)
        {
            _name = name;
            _candidates = new Dictionary<string, Func<Task<T>>>();
        }

        public void RunIf(Func<Task<bool>> block) =>
            _runIf = block;
        public void RunIf(Func<bool> block) =>
            _runIf = () => Task.FromResult(block());

        public void Use(Func<Task<T>> control) =>
            _control = control;

        public void Use(Func<T> control) =>
            _control = () => Task.FromResult(control());

        public void Try(Func<Task<T>> candidate)
        {
            if (_candidates.ContainsKey(CandidateExperimentName))
            {
                throw new InvalidOperationException("You have already added a default try. Give this candidate a new name with the Try(string, Func<Task<T>> overload");
            }
            _candidates.Add(CandidateExperimentName, candidate);
        }

        public void Try(Func<T> candidate)
        {
            if (_candidates.ContainsKey(CandidateExperimentName))
            {
                throw new InvalidOperationException("You have already added a default try. Give this candidate a new name with the Try(string, Func<Task<T>> overload");
            }
            _candidates.Add(CandidateExperimentName, () => Task.FromResult(candidate()));
        }

        public void Try(string name, Func<Task<T>> candidate)
        {
            if (_candidates.ContainsKey(name))
            {
                throw new InvalidOperationException($"You already have a candidate named {name}. Provide a different name for this test.");
            }
            _candidates.Add(name, candidate);
        }

        public void Try(string name, Func<T> candidate)
        {
            if (_candidates.ContainsKey(name))
            {
                throw new InvalidOperationException($"You already have a candidate named {name}. Provide a different name for this test.");
            }
            _candidates.Add(name, () => Task.FromResult(candidate()));
        }

        internal ExperimentInstance<T> Build() =>
            new ExperimentInstance<T>(_name, _control, _candidates, _comparison, _beforeRun, _runIf);

        public void Compare(Func<T, T, bool> comparison)
        {
            _comparison = comparison;
        }

        static readonly Func<T, T, bool> DefaultComparison = (instance, comparand) =>
        {
            return (instance == null && comparand == null)
                || (instance != null && instance.Equals(comparand))
                || (CompareInstances(instance as IEquatable<T>, comparand));
        };

        static bool CompareInstances(IEquatable<T> instance, T comparand) => instance != null && instance.Equals(comparand);

        public void BeforeRun(Action action)
        {
            _beforeRun = async () => { action(); await Task.FromResult(0); };
        }

        public void BeforeRun(Func<Task> action)
        {
            _beforeRun = action;
        }
    }
}
