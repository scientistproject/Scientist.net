using GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Github.Ordering
{
    public delegate Task<IReadOnlyList<INamedBehavior<T>>> CustomOrderer<T>(IReadOnlyList<INamedBehavior<T>> namedBehaviors);

    public class Ordering
    {
        public static IReadOnlyList<INamedBehavior<T>> ControlFirst<T>(IReadOnlyList<INamedBehavior<T>> namedBehaviors)
        {
            return namedBehaviors.OrderByDescending(namedBehavior => namedBehavior.Name == "control").ToList();
        }

        public static IReadOnlyList<INamedBehavior<T>> ControlLast<T>(IReadOnlyList<INamedBehavior<T>> namedBehaviors)
        {
            return ControlFirst(namedBehaviors).Reverse().ToList();
        }

        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        public static IReadOnlyList<INamedBehavior<T>> Random<T>(IReadOnlyList<INamedBehavior<T>> namedBehaviors)
        {
            lock (_random)
            {
                return namedBehaviors.OrderBy(b => _random.Next()).ToArray();
            }
        }
    }

}
