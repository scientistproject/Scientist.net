using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryResultPublisher : IResultPublisher
    {
        readonly static Task _completed = Task.FromResult(0);

        public Task Publish(IResult result)
        {
            Results.Add(result);
            return _completed;
        }

        public ConcurrentBag<IResult> Results { get; } = new ConcurrentBag<IResult>();
    }
}
