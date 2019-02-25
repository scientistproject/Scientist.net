using System.Threading.Tasks;

namespace UnitTests
{
    public interface IScientistSettings
    {
        Task<bool> Enabled();

        Task<bool> EnableControl();
    }
}
