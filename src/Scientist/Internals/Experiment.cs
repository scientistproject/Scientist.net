using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class Experiment<T>
    {
        readonly static Func<Task<bool>> _alwaysRun = () => Task.FromResult(true);

        string _name;
        internal Func<Task<T>> Control { get; set; }
        internal Func<Task<T>> Candidate { get; set; }
        internal Func<T, T, bool> Comparison { get; set; } = DefaultComparison;
        internal Func<Task> BeforeRun { get; set; }
        internal Func<Task<bool>> RunIf { get; set; } = _alwaysRun;

        public Experiment(string name)
        {
            _name = name;
        }
        
        internal ExperimentInstance<T> Build() =>
            new ExperimentInstance<T>(_name, Control, Candidate, Comparison, BeforeRun, RunIf);
        
        static readonly Func<T, T, bool> DefaultComparison = (instance, comparand) =>
        {
            return (instance == null && comparand == null)
                || (instance != null && instance.Equals(comparand))
                || (CompareInstances(instance as IEquatable<T>, comparand));
        };

        static bool CompareInstances(IEquatable<T> instance, T comparand) => instance != null && instance.Equals(comparand);
    }
}
