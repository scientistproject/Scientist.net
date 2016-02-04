using System.Threading.Tasks;

namespace GitHub
{
    public interface IObservationPublisher
    {
        Task Publish(Observation observation);
    }
}