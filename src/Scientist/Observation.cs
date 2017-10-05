using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Defines an observation of an experiment's execution.
    /// </summary>
    public class Observation<T, TClean>
    {
        readonly Func<T, TClean> _cleaner;

        internal Observation(string name, Action<Operation, Exception> thrown, Func<T, TClean> cleaner)
        {
            _cleaner = cleaner;
            ExperimentThrown = thrown;
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
            get;
            private set;
        }

        internal readonly Action<Operation, Exception> ExperimentThrown;

        /// <summary>
        /// Gets the cleaned value of the experiment behavior if successful.
        /// </summary>
        public TClean CleanedValue => _cleaner(Value);

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
            get;
            private set;
        }

        /// <summary>
        /// Determines if another <see cref="Observation{T}"/> matches this instance.
        /// </summary>
        /// <param name="other">The other observation.</param>
        /// <param name="comparator">Used to compare two observations</param>
        /// <returns>True when the observations values/exceptions match.</returns>
        public bool EquivalentTo(Observation<T, TClean> other, Func<T, T, bool> comparator)
        {
            try
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
            catch (Exception ex)
            {
                ExperimentThrown(Operation.Compare, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Creates a new observation, and runs the experiment.
        /// </summary>
        /// <param name="name">The name of the observation.</param>
        /// <param name="block">The experiment to run.</param>
        /// <param name="comparison">The comparison delegate used to determine if an observation is equivalent.</param>
        /// <param name="thrown">The delegate used for handling thrown exceptions during equivalency comparisons.</param>
        /// <returns>The observed experiment.</returns>
        public static async Task<Observation<T, TClean>> New(string name, Func<Task<T>> block, Func<T, T, bool> comparison, Action<Operation, Exception> thrown, Func<T, TClean> cleaner)
        {
            Observation<T, TClean> observation = new Observation<T, TClean>(name, thrown, cleaner);

            await observation.Run(block);

            return observation;
        }

        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        /// <summary>
        /// Runs the experiment.
        /// </summary>
        internal async Task Run(Func<Task<T>> block)
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                Value = await block();
            }
            catch (AggregateException ex)
            {
                Exception = ex.GetBaseException();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            var stop = Stopwatch.GetTimestamp();

            Duration = new TimeSpan((long)(TimestampToTicks * (stop - start)));
        }
    }
}
