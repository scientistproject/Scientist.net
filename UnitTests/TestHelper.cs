using System.Collections.Generic;
using GitHub;
using GitHub.Internals;

public static class TestHelper
{
    public static IEnumerable<Measurement> Measurements =>
        ((InMemoryPublisher)Scientist.MeasurementPublisher).Measurements;
}
