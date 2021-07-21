using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public class InMemoryResultPublisher : IResultPublisher
    {
        readonly Task _completed = Task.FromResult(0);
        readonly ConcurrentDictionary<int, ConcurrentBag<object>> _results 
            = new ConcurrentDictionary<int, ConcurrentBag<object>>();

        static int GetKey<T, TClean>()
        {
            if (typeof(T) == typeof(TClean))
            {
                return typeof(T).TypeHandle.GetHashCode();
            }
            else
            {
                return typeof(T).TypeHandle.GetHashCode()
                ^ typeof(TClean).TypeHandle.GetHashCode();
            }
        }

        public Task Publish<T, TClean>(Result<T, TClean> result)
        {
            _results.AddOrUpdate(
                GetKey<T, TClean>(),
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
        /// <typeparam name="TClean">the type of cleaned result to get.</typeparam>
        /// <returns>All results that have the type provided, and have been published.</returns>
        public IEnumerable<Result<T, TClean>> Results<T, TClean>()
        {
            ConcurrentBag<object> bag;

            // Try to get the list of results.
            if (_results.TryGetValue(GetKey<T, TClean>(), out bag))
            {
                return bag.Cast<Result<T, TClean>>();
            }

            // Otherwise return nothing.
            else { return new Result<T, TClean>[0]; }
        }
    }
}
