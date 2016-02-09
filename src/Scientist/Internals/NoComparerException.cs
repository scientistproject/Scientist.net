using System;

namespace GitHub.Internals
{
    public class NoComparerException : Exception
    {
        public NoComparerException(Type controlType, Type candidateType) 
            : base($"No comparer between types {controlType.Name} and {candidateType.Name}")
        { }
    }
}