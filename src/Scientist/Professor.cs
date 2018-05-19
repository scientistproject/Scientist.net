using System;
using System.Threading.Tasks;
using GitHub.Internals;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths.
    /// </summary>
    public class Professor
    {
        static readonly Task<bool> EnabledTask = Task.FromResult(true);
        int _concurrentTasks = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Professor"/> class
        /// </summary>
        public Professor()
        {
            ResultPublisher = new InMemoryResultPublisher();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Professor"/> class
        /// using the specified <see cref="IResultPublisher"/>.
        /// </summary>
        public Professor(IResultPublisher resultPublisher)
        {
            if (resultPublisher == null)
                throw new ArgumentNullException("A result publisher must be specified", nameof(resultPublisher));

            ResultPublisher = resultPublisher;
        }

        /// <summary>
        /// Gets or sets the number of tasks to run concurrently
        /// </summary>
        public virtual int ConcurrentTasks
        {
            get { return _concurrentTasks; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Argument must be greater than 0", nameof(value));

                _concurrentTasks = value;
            }
        }

        /// <summary>
        /// Gets the results publisher being used
        /// </summary>
        public IResultPublisher ResultPublisher { get; }

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public T Science<T>(string name, Action<IExperiment<T>> experiment)
        {
            var builder = Build<T, T>(name, experiment);
            builder.Clean(value => value);
            return builder.Build().Run().Result;
        }

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public T Science<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment) =>
            Build(name, experiment).Build().Run().Result;

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment)
        {
            var builder = Build<T, T>(name, experiment);
            builder.Clean(value => value);
            return builder.Build().Run();
        }

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public Task<T> ScienceAsync<T, TClean>(string name, Action<IExperimentAsync<T, TClean>> experiment) =>
            Build(name, experiment).Build().Run();

        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether the experiment should be enabled.
        /// </returns>
        protected virtual Task<bool> EnabledAsync() => EnabledTask;

        Experiment<T, TClean> Build<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment)
        {
            var builder = new Experiment<T, TClean>(name, EnabledAsync, _concurrentTasks, ResultPublisher);

            experiment(builder);

            return builder;
        }

        Experiment<T, TClean> Build<T, TClean>(string name, Action<IExperimentAsync<T, TClean>> experiment)
        {
            var builder = new Experiment<T, TClean>(name, EnabledAsync, _concurrentTasks, ResultPublisher);

            experiment(builder);

            return builder;
        }
    }
}
