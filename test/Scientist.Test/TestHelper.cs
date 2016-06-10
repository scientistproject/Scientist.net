using System;
using System.Collections.Generic;
using System.Linq;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using NSubstitute.Core;

public static class TestHelper
{
    public static IEnumerable<Result<T>> Results<T>(string experimentName) => ((InMemoryResultPublisher)Scientist.ResultPublisher).Results<T>().Where(x => x.ExperimentName == experimentName);

    public static ConfiguredCall Throws<T>(this T value, Exception e) => value.Returns(_ => { throw e; });
}
