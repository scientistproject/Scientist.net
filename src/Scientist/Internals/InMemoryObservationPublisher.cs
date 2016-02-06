using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryObservationPublisher : IObservationPublisher
    {
        readonly static Task _completed = Task.FromResult(0);

        public Task Publish(IResult result)
        {
            Observations.Add(result);
            return _completed;
        }

        public ConcurrentBag<IResult> Observations { get; } = new ConcurrentBag<IResult>();
    }
}
