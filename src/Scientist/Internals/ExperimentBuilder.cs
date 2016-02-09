using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T, T1> : IExperiment<T, T1>, IExperimentAsync<T, T1>
    {
        string _name;
        Func<Task<T>> _control;
        Func<Task<T1>> _candidate;
        private Func<T, T1, bool> _comparer;

        public Experiment(string name)
        {
            _name = name;
        }

        public void Use(Func<Task<T>> control) { _control = control; }

        public void Use(Func<T> control) { _control = () => Task.FromResult(control()); }

        public void Try(Func<Task<T1>> candidate) { _candidate = candidate; }

        public void Try(Func<T1> candidate) { _candidate = () => Task.FromResult(candidate()); }

        public void Comparer(Func<T, T1, bool> compareFunc)
        {
            _comparer = compareFunc;
        }

        internal ExperimentInstance<T, T1> Build()
        {
            return new ExperimentInstance<T, T1>(_name, _control, _candidate, _comparer);
        }
    }
}
