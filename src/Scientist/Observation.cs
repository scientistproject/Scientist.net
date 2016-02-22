using NullGuard;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Defines an observation of an experiment's execution.
    /// </summary>
    public class Observation<T>
    {
        internal Observation(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the total time that the experiment behavior ran for.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Gets an exception if one was thrown from the experiment behavior.
        /// </summary>
        public Exception Exception
        {
#if net451
            [return: AllowNull]
#endif
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the experiment behavior.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether an exception was observed.
        /// </summary>
        public bool Thrown => Exception != null;

        /// <summary>
        /// Gets the value of the experiment behavior if successful.
        /// </summary>
        public T Value
        {
#if net451
            [return: AllowNull]
#endif
            get;
            private set;
        }

        /// <summary>
        /// Determines if another <see cref="Observation{T}"/> matches this instance.
        /// </summary>
        /// <param name="other">The other observation.</param>
        /// <param name="comparator">Used to compare two observations</param>
        /// <returns>True when the observations values/exceptions match.</returns>
        public bool EquivalentTo(Observation<T> other, Func<T, T, bool> comparator)
        {
            bool valuesAreEqual = false;
            bool bothRaised = other.Thrown && Thrown;
            bool neitherRaised = !other.Thrown && !Thrown;

            if (neitherRaised)
            {
                // TODO if block_given?
                valuesAreEqual = comparator(other.Value, Value);
            }

            bool exceptionsAreEquivalent =
                bothRaised &&
                other.Exception.GetType() == Exception.GetType() &&
                other.Exception.Message == Exception.Message;

            return (neitherRaised && valuesAreEqual) ||
                (bothRaised && exceptionsAreEquivalent);
        }
        
        /// <summary>
        /// Creates a new observation, and runs the experiment.
        /// </summary>
        /// <param name="name">The name of the observation.</param>
        /// <param name="block">The experiment to run.</param>
        /// <returns>The observed experiment.</returns>
        public static async Task<Observation<T>> New(string name, Func<Task<T>> block, Func<T, T, bool> comparison)
        {
            Observation<T> observation = new Observation<T>(name);

            await observation.Run(block);

            return observation;
        }

        /// <summary>
        /// Runs the experiment.
        /// </summary>
        internal async Task Run(Func<Task<T>> block)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Value = await block();
            }
            catch (Exception ex)
            {
                Exception = ex.GetBaseException();
            }
            stopwatch.Stop();

            Duration = stopwatch.Elapsed;
        }
    }
}
