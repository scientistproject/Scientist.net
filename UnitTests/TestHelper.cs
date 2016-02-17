using System;
using System.Collections.Generic;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using NSubstitute.Core;

public static class TestHelper
{
    public static IEnumerable<Result<T>> Results<T>() => ((InMemoryResultPublisher)Scientist.ResultPublisher).Results<T>();

    public static ConfiguredCall Throws<T>(this T value, Exception e) => value.Returns(_ => { throw e; });
}
