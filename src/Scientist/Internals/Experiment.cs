using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class Experiment<T> : IExperiment<T>, IExperimentAsync<T>
    {
        string _name;
        Func<Task<T>> _control;
        Func<Task<T>> _candidate;
        Func<T, T, bool> _comparison = DefaultComparison;

        public Experiment(string name)
        {
            _name = name;
        }

        public void Use(Func<Task<T>> control) =>
            _control = control;

        public void Use(Func<T> control) =>
            _control = () => Task.FromResult(control());

        // TODO add optional name parameter, and store all delegates into a dictionary.

        public void Try(Func<Task<T>> candidate) =>
            _candidate = candidate;

        public void Try(Func<T> candidate) =>
            _candidate = () => Task.FromResult(candidate());

        internal ExperimentInstance<T> Build() =>
            new ExperimentInstance<T>(_name, _control, _candidate, _comparison);

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
    }
}