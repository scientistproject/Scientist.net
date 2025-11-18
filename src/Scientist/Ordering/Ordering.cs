using GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Github.Ordering
{
    public delegate Task<IReadOnlyList<INamedBehaviour<T>>> CustomOrderer<T>(IReadOnlyList<INamedBehaviour<T>> namedBehaviours);

    public class Ordering
    {
        public static IReadOnlyList<INamedBehaviour<T>> ControlFirst<T>(IReadOnlyList<INamedBehaviour<T>> namedBehaviours)
        {
            return namedBehaviours.OrderByDescending(namedBehaviour => namedBehaviour.Name == "control").ToList();
        }

        public static IReadOnlyList<INamedBehaviour<T>> ControlLast<T>(IReadOnlyList<INamedBehaviour<T>> namedBehaviours)
        {
            return ControlFirst(namedBehaviours).Reverse().ToList();
        }

        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        public static IReadOnlyList<INamedBehaviour<T>> Random<T>(IReadOnlyList<INamedBehaviour<T>> namedBehaviours)
        {
            lock (_random)
            {
                return namedBehaviours.OrderBy(b => _random.Next()).ToArray();
            }
        }
    }

}
