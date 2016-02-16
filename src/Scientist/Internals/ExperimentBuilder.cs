using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T> : IExperiment<T>, IExperimentAsync<T>
    {
        string _name;
        Func<Task<T>> _control;
        Func<Task<T>> _candidate;
        Func<Task> _beforeRun;

        public Experiment(string name)
        {
            _name = name;
        }

        public void Use(Func<Task<T>> control) { _control = control; }

        public void Use(Func<T> control) { _control = () => Task.FromResult(control()); }


        public void Try(Func<Task<T>> candidate) { _candidate = candidate; }

        public void Try(Func<T> candidate) { _candidate = () => Task.FromResult(candidate()); }

        internal ExperimentInstance<T> Build()
        {
            return new ExperimentInstance<T>(_name, _control, _candidate, _beforeRun);
        }

        public void BeforeRun(Action action)
        {
            if (action != null)
            {
                _beforeRun = async () => { action(); await Task.FromResult(0); };
            }
        }

        public void BeforeRun(Func<Task> action) { _beforeRun = action; }
    }
}
