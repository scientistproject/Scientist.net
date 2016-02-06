using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public interface IExperiment<T>
    {
        void Try(Func<T> candidate);

        void Use(Func<T> control);

        Func<T, T, bool> ResultComparison { get; set; }

        IEqualityComparer<T> ResultEqualityCompare { get; set; }

    }

    public interface IExperimentAsync<T>
    {
        void Try(Func<Task<T>> candidate);

        void Use(Func<Task<T>> control);

    }

}