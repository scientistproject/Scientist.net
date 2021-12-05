using System;
using System.Collections.Generic;
using System.Linq;
using Scientist.Internals;
using NSubstitute;
using NSubstitute.Core;

namespace Scientist.Test
{
    public static class TestHelper
    {
        public static IEnumerable<Result<T, T>> Results<T>(string experimentName) => Scientist.ResultPublisher.Results<T, T>(experimentName);
        public static IEnumerable<Result<T, TClean>> Results<T, TClean>(string experimentName) => Scientist.ResultPublisher.Results<T, TClean>(experimentName);
        public static IEnumerable<Result<T, T>> Results<T>(this IResultPublisher publisher, string experimentName) => publisher.Results<T, T>(experimentName);
        public static IEnumerable<Result<T, TClean>> Results<T, TClean>(this IResultPublisher publisher, string experimentName) => ((InMemoryResultPublisher)publisher).Results<T, TClean>().Where(x => x.ExperimentName == experimentName);

        public static ConfiguredCall Throws<T>(this T value, Exception e) => value.Returns(_ => { throw e; });
    }
}
