using System.Collections.Generic;
using GitHub;
using GitHub.Internals;

namespace UnitTests
{
    using GitHub;

    public static class TestHelper
    {
        public static IEnumerable<Observation> Observation =>
            ((InMemoryObservationPublisher) Scientist.ObservationPublisher).Observations;
    }
}