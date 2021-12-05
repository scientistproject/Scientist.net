using System.Threading.Tasks;

namespace Scientist.Test
{
    public interface IScientistSettings
    {
        Task<bool> Enabled();
    }
}
