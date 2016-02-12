using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using GitHub;
using GitHub.Internals;

public static class TestHelper
{
    public static IEnumerable<Observation> Observation =>
        ((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IQbservable<Observation> ObservationsGeneratedInThisMethod([CallerMemberName] string callingMethodName = "")
    {
        return Scientist.Observations.Where(w => w.CallingMethodName.Equals(callingMethodName));
    }
}
