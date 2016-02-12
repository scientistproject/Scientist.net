using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Internals;
using NullGuard;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public static class Scientist
    {
        // TODO: Evaluate the distribution of Random and whether it's good enough.
        static readonly Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        private static readonly BlockingCollection<Observation> ObservationsToPublish = new BlockingCollection<Observation>();
        private static readonly Lazy<IQbservable<Observation>> SetupObservations = new Lazy<IQbservable<Observation>>(
            () =>
            {
                var qbservations = ObservationsToPublish.ToObservable(TaskPoolScheduler.Default).AsQbservable();
                qbservations.Subscribe((observation) => { ObservationPublisher.Publish(observation); });// So ObservationPublisher still works

                return qbservations;

            });

        public static readonly IQbservable<Observation> Observations = SetupObservations.Value;


        // Should be configured once before starting observations.
        public static IObservationPublisher ObservationPublisher { get; set; } = new InMemoryObservationPublisher();

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
       
        /// <returns>The value of the experiment's control function.</returns>
        [return: AllowNull]
        [MethodImpl(MethodImplOptions.NoInlining)] //So that we get the Source code CallerMemberName method name (may be lost when inlined in Release Mode).
#pragma warning disable 1573
        public static T Science<T>(string name, Action<IExperiment<T>> experiment, [CallerMemberName] string callingMethodName = "")
#pragma warning restore 1573
        {
            var experimentBuilder = new Experiment<T>(name, callingMethodName);

            experiment(experimentBuilder);

            return experimentBuilder.Build().Run().Result;
        }

        /// <summary>
        /// Conduct an asynchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
        [return: AllowNull]
        [MethodImpl(MethodImplOptions.NoInlining)] //So that we get the Source code CallerMemberName method name (may be lost when inlined in Release Mode).
#pragma warning disable 1573
        public static Task<T> ScienceAsync<T>(string name, Action<IExperimentAsync<T>> experiment, [CallerMemberName] string callingMethodName = "")
#pragma warning restore 1573
        {
            var experimentBuilder = new Experiment<T>(name, callingMethodName);

            experiment(experimentBuilder);

            return experimentBuilder.Build().Run();
        }

        /// <summary>
        /// Fast, fire and forget, publishing of Observations <see cref="Observation"/> .
        /// </summary>
        /// <param name="observation">Observation to publish</param>
        public static void PublishObservation(Observation observation)
        {
            ObservationsToPublish.Add(observation);
        }
    }
}
