using System.Threading.Tasks;

namespace GitHub
{
    public interface IMeasurementPublisher
    {
        Task Publish(Measurement measurement);
    }
}