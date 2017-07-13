using System;

namespace GitHub
{
    /// <summary>
    /// Thrown when an exception was thrown during the execution of an operation.
    /// </summary>
    public class OperationException : Exception
    {
        /// <summary>
        /// The operation in which this exception occurred within.
        /// </summary>
        public Operation Operation { get; }

        internal OperationException(Operation operation, Exception inner) : base(GetExceptionMessage(operation), inner)
        {
            Operation = operation;
        }

        private static string GetExceptionMessage(Operation operation)
        {
            string exceptionMessage = Enum.IsDefined(typeof(Operation), operation)
                ? $"An exception occurred within the experiment during the '{operation}' operation. Consider checking both the inner exception and the {nameof(Operation)} property for details on this exception."
                : "An exception occurred within the experiment, the operation was unknown. The operation should never be unknown, please report this.";
            return exceptionMessage;
        }
    }
}