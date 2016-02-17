using NullGuard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Defines an observation of an experiment's execution.
    /// </summary>
    public class Observation<T> : IEquatable<Observation<T>>
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
            [return: AllowNull]
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
            [return: AllowNull]
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Observation<T>;
            if (other == null) { return false; }
            else { return Equals(other); }
        }

        /// <summary>
        /// Determines if another <see cref="Observation{T}"/> matches this instance.
        /// </summary>
        /// <param name="other">The other observation.</param>
        /// <returns>True when the observations values/exceptions match.</returns>
        public bool Equals(Observation<T> other)
        {
            bool valuesAreEqual = false;
            bool bothRaised = other.Thrown && Thrown;
            bool neitherRaised = !other.Thrown && !Thrown;

            if (neitherRaised)
            {
                // TODO if block_given?
                valuesAreEqual = (other.Value == null && Value == null) ||
                    other.Value.Equals(Value);
            }

            bool exceptionsAreEquivalent =
                bothRaised &&
                other.Exception.GetType() == Exception.GetType() &&
                other.Exception.Message == Exception.Message;

            return (neitherRaised && valuesAreEqual) ||
                (bothRaised && exceptionsAreEquivalent);
        }
        
        /// <summary>
        /// Calculates the unique hash code for the observation.
        /// </summary>
        public override int GetHashCode()
        {
            IEnumerable<int> hashCodes = new object[] { Value, Exception, typeof(Observation<T>) }
                .Where(o => o != null).Select(o => o.GetHashCode());

            int? result = null;
            foreach (int hashCode in hashCodes)
            {
                if (result.HasValue) { result = result.Value ^ hashCode; }
                else { result = hashCode; }
            }

            return result.Value;
        }

        public static bool operator ==(Observation<T> o1, Observation<T> o2)
        {
            return o1?.Equals(o2) ?? false;
        }

        public static bool operator !=(Observation<T> o1, Observation<T> o2)
        {
            return !(o1?.Equals(o2)) ?? true;
        }

        /// <summary>
        /// Creates a new observation, and runs the experiment.
        /// </summary>
        /// <param name="name">The name of the observation.</param>
        /// <param name="block">The experiment to run.</param>
        /// <returns>The observed experiment.</returns>
        public static async Task<Observation<T>> New(string name, Func<Task<T>> block)
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
