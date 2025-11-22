using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub
{
    public class Candidate<T>
    {
        public Candidate(Func<Task<T>> behavior, CancellationToken cancellationToken = default)
        {
            Behavior = behavior;
            CancellationToken = cancellationToken;
        }
        public Func<Task<T>> Behavior { get; }
        public CancellationToken CancellationToken { get; }
    }
}
