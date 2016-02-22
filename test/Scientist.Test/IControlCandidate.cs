using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public interface IControlCandidateTask<T>
    {
        Task<T> Control();
        Task<T>  Candidate();
        void BeforeRun();
    }
}
