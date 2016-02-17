using System.Collections.Generic;
using GitHub;
using GitHub.Internals;

public static class TestHelper
{
    public static IEnumerable<Result<T>> Results<T>()
    {
        return ((InMemoryResultPublisher)Scientist.ResultPublisher).Results<T>();
    }
}
