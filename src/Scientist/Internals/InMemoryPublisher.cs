using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryPublisher : IMeasurementPublisher
    {
        readonly ConcurrentBag<Measurement> _measurements = new ConcurrentBag<Measurement>();

        public Task Publish(Measurement measurement)
        {
            Measurements.Add(measurement);
            return Task.FromResult(0);
        }

        public ConcurrentBag<Measurement> Measurements { get; }
    }
}
