using System.Collections.Generic;
using GitHub;
using GitHub.Internals;

public static class TestHelper
{
    public static IEnumerable<Observation> Observation =>
        ((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations;
}