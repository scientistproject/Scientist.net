using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// Provides an interface for global settings to science experiments.
    /// </summary>
    public interface ILaboratory
    {
        /// <summary>
        /// Determines if an experiment should be enabled.
        /// </summary>
        /// <returns>An asynchronous task to determine if an experiment should run.</returns>
        Task<bool> Enabled();
    }
}
