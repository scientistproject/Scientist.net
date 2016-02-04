using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public interface IExperiment<T>
    {
        void Try(Func<T> candidate);

        void Use(Func<T> control);
    }

    public interface IExperimentAsync<T>
    {
        void Try(Func<Task<T>> candidate);

        void Use(Func<Task<T>> control);
    }

}