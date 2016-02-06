using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Provides an interface for publishing observed experiments.
    /// </summary>
    public interface IObservationPublisher
    {
        /// <summary>
        /// Publishes the results of an experiment.
        /// </summary>
        /// <param name="result">The result of the experiment.</param>
        /// <returns>A task that publishes the results asynchronously.</returns>
        Task Publish(IResult result);
    }
}