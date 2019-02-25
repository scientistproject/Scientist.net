using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// Declares all of the settings necessary in order
    /// to create a new <see cref="ExperimentInstance{T, TClean}"/>.
    /// </summary>
    /// <typeparam name="T">The result type for the experiment.</typeparam>
    /// <typeparam name="TClean">The cleaned type of the experiment.</typeparam>
    internal class ExperimentSettings<T, TClean>
    {
        public Func<Task> BeforeRun { get; set; }
        public Dictionary<string, Func<Task<T>>> Candidates { get; set; }
        public Func<T, TClean> Cleaner { get; set; }
        public Func<T, T, bool> Comparator { get; set; }
        public Dictionary<string, dynamic> Contexts { get; set; }
        public Func<Task<T>> Control { get; set; }
        public Func<Task<bool>> Enabled { get; set; }
        public Func<Task<bool>> EnableControl { get; set; }
        public IEnumerable<Func<T, T, Task<bool>>> Ignores { get; set; }
        public string Name { get; set; }
        public int ConcurrentTasks { get; set; }
        public Func<Task<bool>> RunIf { get; set; }
        public bool ThrowOnMismatches { get; set; }
        public Action<Operation, Exception> Thrown { get; set; }
        public IResultPublisher ResultPublisher { get; set; }
    }
}
