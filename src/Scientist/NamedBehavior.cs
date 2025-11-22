using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub
{
    public interface INamedBehavior<T>
    {
        string Name { get; }
        Func<Task<T>> Behavior { get; }
        CancellationToken CancellationToken { get; }
    }
    public class NamedBehavior<T> : INamedBehavior<T>
    {
        public NamedBehavior(string name, Func<T> behavior)
        {
            Name = name;
            Behavior = () => Task.FromResult(behavior());
        }

        public NamedBehavior(string name, Func<Task<T>> behavior, CancellationToken cancellationToken)
        {
            Behavior = behavior;
            Name = name;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the behavior to execute during an experiment.
        /// </summary>
        public Func<Task<T>> Behavior { get; }

        /// <summary>
        /// Gets the cancellation token to use during an experiment.
        /// </summary>
        public CancellationToken CancellationToken { get; }

    }
}
