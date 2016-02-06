using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryObservationPublisher : IObservationPublisher
    {
        public Task Publish(Observation observation)
        {
            Observations.Add(observation);
            return Task.FromResult(0);
        }

        public ConcurrentBag<Observation> Observations { get; } = new ConcurrentBag<Observation>();
    }
}
