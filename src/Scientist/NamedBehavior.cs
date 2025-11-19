using System;
using System.Threading.Tasks;

namespace GitHub
{
    public interface INamedBehavior<T>
    {
        string Name { get; }
        Func<Task<T>> Behavior { get; }
    }
    public class NamedBehavior<T> : INamedBehavior<T>
    {
        public NamedBehavior(string name, Func<T> method)
                : this(name, () => Task.FromResult(method()))
        {
        }

        public NamedBehavior(string name, Func<Task<T>> method)
        {
            Behavior = method;
            Name = name;
        }

        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the behavior to execute during an experiment.
        /// </summary>
        public Func<Task<T>> Behavior { get; }
    }
}
