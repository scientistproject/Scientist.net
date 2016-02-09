using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public interface IExperiment<T,T1>
    {
        void Try(Func<T1> candidate);

        void Use(Func<T> control);

        void Comparer(Func<T, T1, bool> compareFunc);
    }

    public interface IExperimentAsync<T,T1>
    {
        void Try(Func<Task<T1>> candidate);

        void Use(Func<Task<T>> control);

        void Comparer(Func<T, T1, bool> func);
    }

}