using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// Implements laboratory, and always enables all experiments.
    /// </summary>
    public class DefaultLaboratory : ILaboratory
    {
        readonly Task<bool> _completed = Task.FromResult(true);

        public Task<bool> Enabled() => _completed;
    }
}
