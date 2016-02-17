using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T> : Experiment<T, T>, IExperiment<T>, IExperimentAsync<T>
    {
        public Experiment(string name)
            :base(name)
        {
            _name = name;
        }
    }

    internal class Experiment<TControl, TCandidate> : IExperiment<TControl, TCandidate>, IExperimentAsync<TControl, TCandidate>
    {
        protected string _name;
        Func<Task<TControl>> _control;
        Func<Task<TCandidate>> _candidate;

        public Experiment(string name)
        {
            _name = name;
        }

        public void Use(Func<Task<TControl>> control) { _control = control; }

        public void Use(Func<TControl> control) { _control = () => Task.FromResult(control()); }


        public void Try(Func<Task<TCandidate>> candidate) { _candidate = candidate; }

        public void Try(Func<TCandidate> candidate) { _candidate = () => Task.FromResult(candidate()); }

        internal ExperimentInstance<TControl, TCandidate> Build()
        {
            return new ExperimentInstance<TControl, TCandidate>(_name, _control, _candidate);
        }
    }
}
