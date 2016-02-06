using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Provides an interface for publishing observed experiments.
    /// </summary>
    public interface IObservationPublisher
    {
        /// <summary>
        /// Publishes the results of an experiment's observation.
        /// </summary>
        /// <param name="observation">The observation of an experiment's run.</param>
        /// <returns>A task that publishes the observation's results asynchronously.</returns>
        Task Publish(Observation observation);
    }
}