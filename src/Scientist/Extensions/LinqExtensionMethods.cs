using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.Internals.Extensions
{
    internal static class LinqExtensionMethods
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        internal static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            if (chunksize <= 0)
                throw new ArgumentException("Argument must be greater than 0", "chunkSize");

            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}