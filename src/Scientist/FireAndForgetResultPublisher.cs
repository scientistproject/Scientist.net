using GitHub.Internals;
using System;
using System.Threading.Tasks;

namespace GitHub
{
    public enum ResultPublisherExceptionBehavior
    {
        Rethrow,
        Ignore
    }

    public class FireAndForgetResultPublisher : IResultPublisher
    {
        private readonly IResultPublisher _publisher;
        private readonly Action<Exception> _onPublisherException;
        private readonly ConcurrentSet<Task> _publishingTasks = new ConcurrentSet<Task>();
        private readonly ResultPublisherExceptionBehavior _exceptionBehavior;

        public FireAndForgetResultPublisher(IResultPublisher publisher, ResultPublisherExceptionBehavior exceptionBehavior, Action<Exception> onPublisherException = null)
        {
            _publisher = publisher;
            _onPublisherException = onPublisherException;
            _exceptionBehavior = exceptionBehavior;
        }

        public Task Publish<T, TClean>(Result<T, TClean> result)
        {

            // Disable the warning, because the task is being tracked on the set, 
            // and immediately removed upon completion.
#pragma warning disable CS4014
            var subTask = Task.Run(async () =>
            {
                try
                {
                    await _publisher.Publish(result);
                }
                catch(Exception ex)
                {
                    _onPublisherException?.Invoke(ex);
                    if(_exceptionBehavior == ResultPublisherExceptionBehavior.Rethrow)
                        throw;
                }
            });
            _publishingTasks.TryAdd(subTask);
            subTask.ContinueWith(_ =>
            {
                _publishingTasks.TryRemove(subTask);
            });
#pragma warning restore CS4014
            return Task.FromResult((object)null);
        }

        public Task WhenPublished() => 
            Task.WhenAll(_publishingTasks.Items);
    }
}
