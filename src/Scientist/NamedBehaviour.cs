using System;
using System.Threading.Tasks;

namespace GitHub
{
    public interface INamedBehaviour<T> {
        string Name { get; }
        Func<Task<T>> Behaviour { get; }
    }
    public class NamedBehaviour<T>: INamedBehaviour<T>
    {
        public NamedBehaviour(string name, Func<T> method)
                : this(name, () => Task.FromResult(method()))
        {
        }   

        public NamedBehaviour(string name, Func<Task<T>> method)
        {
            Behaviour = method;
            Name = name;
        }

        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the behavior to execute during an experiment.
        /// </summary>
        public Func<Task<T>> Behaviour { get; }        
    }
}
