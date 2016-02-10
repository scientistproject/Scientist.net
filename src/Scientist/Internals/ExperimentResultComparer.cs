using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    internal class ExperimentResultComparer<T> : IEqualityComparer<ExperimentInstance<T>.ExperimentResult>
    {

        private readonly Func<T, T, bool> _resultComparison;
        private readonly IEqualityComparer<T> _resultEqualityComparer;

        public ExperimentResultComparer(IEqualityComparer<T> resultEqualityComparer, Func<T, T, bool> resultComparison)
        {
            _resultEqualityComparer = resultEqualityComparer;
            _resultComparison = resultComparison;
        }

       
   


        private bool BothResultsAreNull(T controlResult, T candidateResult)
        {
            return controlResult == null && candidateResult == null;
        }
        private bool BothResultsEqual(T controlResult, T candidateResult)
        {
            return controlResult != null && controlResult.Equals(candidateResult);
        }


        private bool ExceptionsAreEqual(Exception controlException, Exception candidateException)
        {
            //*Both observations raised an exception with the same class and message.
            bool bothExceptionsSameType = controlException != null && controlException.GetType().FullName.Equals(candidateException.GetType().FullName);
            bool bothExceptionsSameMessage = controlException != null && controlException.Message.Equals(candidateException.Message);

            return bothExceptionsSameType && bothExceptionsSameMessage;
        }

        /// <summary>
        /// Checks if two ExperimentResults are equal
        /// </summary>
        /// <param name="controlResult">Control ExperimentResult</param>
        /// <param name="candidateResult">Candidate ExperimentResult</param>
        /// <returns>
        ///  Returns true if: 
        /// 
        ///  The values of the observations are equal (using .Equals()) 
        ///  The values of the observations are equal according to Ts IEquatable&lt;T&gt; implementation, if implemented
        ///  The values of the observations are equal according to a comparison function, if given
        ///  The values of the observations are equal according to an IEqualityComparer&lt;T&gt; expression, if given  
        ///  Both observations raised an exception with the same Type and message.
        ///  Both values of the observation are null
        ///  
        ///  Returns false otherwise. 
        /// </returns>
        public bool Equals(ExperimentInstance<T>.ExperimentResult controlResult, ExperimentInstance<T>.ExperimentResult candidateResult)
        {
            if (_resultComparison != null)
            {
                return _resultComparison(controlResult.Result, candidateResult.Result);
            }

            if (_resultEqualityComparer != null)
            {
                return _resultEqualityComparer.Equals(controlResult.Result, candidateResult.Result);
            }

            var equatableResult = controlResult.Result as IEquatable<T>;
            if (equatableResult != null)
            {
                return equatableResult.Equals(candidateResult.Result);
            }


            bool success =
                  BothResultsAreNull(controlResult.Result, candidateResult.Result)
               || BothResultsEqual(controlResult.Result, candidateResult.Result)
               || ExceptionsAreEqual(controlResult.ThrownException, candidateResult.ThrownException);


            return success;
        }

        public int GetHashCode(ExperimentInstance<T>.ExperimentResult obj)
        {
            return obj.GetHashCode();
        }
    }
}
