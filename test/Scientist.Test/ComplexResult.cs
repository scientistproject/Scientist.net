using System;

public class ComplexResult : IEquatable<ComplexResult>
{
    public int Count { get; set; }
    public string Name { get; set; }

    public bool Equals(ComplexResult other)
    {
        if (other == null)
            return false;

        return Count == other.Count && Name == other.Name;
    }
}