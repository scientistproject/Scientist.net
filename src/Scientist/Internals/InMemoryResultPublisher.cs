using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryResultPublisher : IResultPublisher
    {
        readonly static Task _completed = Task.FromResult(0);
        readonly static ConcurrentDictionary<Type, ConcurrentBag<object>> _results 
            = new ConcurrentDictionary<Type, ConcurrentBag<object>>();

        public Task Publish<T>(Result<T> result)
        {
            _results.AddOrUpdate(
                typeof(T),
                key => new ConcurrentBag<object>
                {
                    result
                },
                (key, value) =>
                {
                    value.Add(result);
                    return value;
                });
            return _completed;
        }

        /// <summary>
        /// Gets the results of a specific type from the publisher.
        /// </summary>
        /// <typeparam name="T">The type of result to get.</typeparam>
        /// <returns>All results that have the type provided, and have been published.</returns>
        public IEnumerable<Result<T>> Results<T>()
        {
            ConcurrentBag<object> bag;

            // Try to get the list of results.
            if (_results.TryGetValue(typeof(T), out bag))
            {
                return bag.Cast<Result<T>>();
            }

            // Otherwise return nothing.
            else { return new Result<T>[0]; }
        }
    }
}
