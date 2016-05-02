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

    public interface IControlCandidateTask<T>
    {
        Task<T> Control();
        Task<T>  Candidate();
        void BeforeRun();
    }
}
