using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public interface IExperiment<T>
    {
        void Try(Func<Task<T>> candidate);
        void Try(Func<T> candidate);
        void Use(Func<Task<T>> control);
        void Use(Func<T> control);

        Func<T, T, bool> ResultComparison { get; set; } 
    }
}