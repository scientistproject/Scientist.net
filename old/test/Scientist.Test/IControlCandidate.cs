using GitHub;
using System;
using System.Threading.Tasks;

namespace UnitTests
{
    public interface IControlCandidate
    {
        void Control();
        void Candidate();
        void BeforeRun();
    }

    public interface IControlCandidate<out T>
    {
        T Control();
        T Candidate();
        void BeforeRun();
        bool RunIf();
        void Thrown(Operation operation, Exception exception);
    }

    public interface IControlCandidate<T, TClean> : IControlCandidate<T>
    {
        TClean Clean(T value);
    }

    public interface IControlCandidateTask<T>
    {
        Task<T> Control();
        Task<T>  Candidate();
        void BeforeRun();
    }

    public interface IControlCandidateTask<T, TClean> : IControlCandidateTask<T>
    {
        TClean Clean(T value);
    }
}
