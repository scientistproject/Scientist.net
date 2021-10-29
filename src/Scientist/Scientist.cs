using System;
using System.Threading.Tasks;
using GitHub.Internals;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public class Scientist : IScientist
    {
        static readonly Task<bool> EnabledTask = Task.FromResult(true);
        static readonly Task<bool> IsEnabledControl = Task.FromResult(true);
        
        static readonly Lazy<Scientist> _sharedScientist = new Lazy<Scientist>(CreateSharedInstance);
        static Func<Task<bool>> _enabled = () => EnabledTask;
        static Func<Task<bool>> _enabledControl = () => IsEnabledControl;
        static IResultPublisher _sharedPublisher = new InMemoryResultPublisher();
        readonly IResultPublisher _resultPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scientist"/> class
        /// using the specified <see cref="IResultPublisher"/>.
        /// </summary>
        public Scientist(IResultPublisher resultPublisher)
        {
            _resultPublisher = resultPublisher
                ?? throw new ArgumentNullException(nameof(resultPublisher), "A result publisher must be specified");
        }

        // TODO: How can we guide the developer to the pit of success

        /// <summary>
        /// Gets or sets the result publisher to use.
        /// This should be configured once before starting observations.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// An attempt to set the value was made after the first experiment has been run.
        /// </exception>
        public static IResultPublisher ResultPublisher
        {
            get
            {
                return _sharedPublisher;
            }

            set
            {
                if (_sharedScientist.IsValueCreated)
                {
                    throw new InvalidOperationException($"The value of the {nameof(ResultPublisher)} property cannot be changed once an experiment has been run.");
                }

                _sharedPublisher = value;
            }
        }

        static Scientist CreateSharedInstance() => new SharedScientist(ResultPublisher);

        Experiment<T, TClean> Build<T, TClean>(string name, int concurrentTasks, Action<IExperiment<T, TClean>> experiment)
        {
            // TODO: Maybe we could automatically generate the name if none is provided using the calling method name. We'd have to 
            // make sure we don't inline this method though.
            var experimentBuilder = new Experiment<T, TClean>(name, IsEnabledAsync, IsEnabledControlAsync ,concurrentTasks, _resultPublisher);

            experiment(experimentBuilder);

            return experimentBuilder;
        }

        Experiment<T, TClean> Build<T, TClean>(string name, int concurrentTasks, Action<IExperimentAsync<T, TClean>> experiment)
        {
            var builder = new Experiment<T, TClean>(name, IsEnabledAsync, IsEnabledControlAsync, concurrentTasks, _resultPublisher);

            experiment(builder);

            return builder;
        }

        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <param name="enabled">A delegate returning if an experiment should run.</param>
        public static void Enabled(Func<bool> enabled) => Enabled(() => Task.FromResult(enabled()));

        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <param name="enabled">A delegate returning an asynchronous task determining if an experiment should run.</param>
        public static void Enabled(Func<Task<bool>> enabled) => _enabled = enabled;


        public static void EnabledControl(Func<bool> enabledControl) => EnabledControl(() => Task.FromResult(enabledControl()));

        /// <summary>
        /// Determines if the control method should be enabled.
        /// </summary>
        /// <param name="enabled">A delegate returning an asynchronous task determining if the control method should run.</param>
        public static void EnabledControl(Func<Task<bool>> enabledControl) => _enabledControl = enabledControl;

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Science<T>(string name, Action<IExperiment<T>> experiment) =>
            _sharedScientist.Value.Experiment(name, experiment);

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static T Science<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment) =>
            _sharedScientist.Value.Experiment(name, experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment) =>
            _sharedScientist.Value.ExperimentAsync(name, experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T>(string name, int concurrentTasks, Action<IExperimentAsync<T>> experiment) =>
            _sharedScientist.Value.ExperimentAsync(name, concurrentTasks, experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T, TClean>(string name, Action<IExperimentAsync<T, TClean>> experiment) =>
            _sharedScientist.Value.ExperimentAsync(name, experiment);

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public static Task<T> ScienceAsync<T, TClean>(string name, int concurrentTasks, Action<IExperimentAsync<T, TClean>> experiment) =>
            _sharedScientist.Value.ExperimentAsync(name, concurrentTasks, experiment);

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public T Experiment<T, TClean>(string name, Action<IExperiment<T, TClean>> experiment) =>
            Build(name, 1, experiment).Build().Run().Result;

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <typeparam name="TClean">The clean type for publishing.</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="concurrentTasks">Number of tasks to run concurrently</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        public Task<T> ExperimentAsync<T, TClean>(string name, int concurrentTasks, Action<IExperimentAsync<T, TClean>> experiment) =>
            Build(name, concurrentTasks, experiment).Build().Run();

        /// <summary>
        /// Returns whether the experiment is enabled as an asynchronous operation
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> which returns whether the experiment is enabled.
        /// </returns>
        /// <remarks>
        /// Override this method to change the default implementation which always returns <see langword="true"/>.
        /// </remarks>
        protected virtual Task<bool> IsEnabledAsync() => EnabledTask;

        protected virtual Task<bool> IsEnabledControlAsync() => IsEnabledControl;

        /// <summary>
        /// This class acts as a proxy to allow the static methods to set the state on an instance of Scientist.
        /// </summary>
        private sealed class SharedScientist : Scientist
        {
            internal SharedScientist(IResultPublisher resultPublisher)
                : base(resultPublisher)
            {
            }

            protected override async Task<bool> IsEnabledAsync()
            {
                return await _enabled().ConfigureAwait(false);
            }

            protected override async Task<bool> IsEnabledControlAsync()
            {
                return await _enabledControl();
            }
        }
    }
}
