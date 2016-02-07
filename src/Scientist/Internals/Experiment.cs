using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T> : IExperiment<T>, IExperimentAsync<T>
    {
        readonly static Func<Task<bool>> _alwaysRun = () => Task.FromResult(true);

        string _name;
        Func<Task<T>> _control;
        Func<Task<T>> _candidate;
        Func<Task<bool>> _runIf = _alwaysRun;

        public Experiment(string name)
        {
            _name = name;
        }

        public void RunIf(Func<Task<bool>> block) { _runIf = block; }
        public void RunIf(Func<bool> block) { _runIf = () => Task.FromResult(block()); }

        public void Use(Func<Task<T>> control) { _control = control; }

        public void Use(Func<T> control) { _control = () => Task.FromResult(control()); }

        // TODO add optional name parameter, and store all delegates into a dictionary.

        public void Try(Func<Task<T>> candidate) { _candidate = candidate; }

        public void Try(Func<T> candidate) { _candidate = () => Task.FromResult(candidate()); }

        internal ExperimentInstance<T> Build()
        {
            return new ExperimentInstance<T>(_name, _control, _candidate, _runIf);
        }
    }
}
