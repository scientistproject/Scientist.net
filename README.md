# Scientist.NET

This is a .NET Port of the Ruby Scientist library. Read [about it here](http://githubengineering.com/scientist/). And check out a great [success story here](http://githubengineering.com/move-fast/).

Currently, it's a rough sketch of what the library will look like. I tried to stay true to the Ruby implementation with one small difference. Instead of registering a custom experimentation type, you can register a custom observation publisher. We don't have the ability to override the `new` operator like those Rubyists and I liked keeping publishing separate. But I'm not stuck to this idea.

Here's a sample usage.


```csharp
public bool MayPush(IUser user)
{
    return Scientist.Science<bool>("experiment-name", experiment =>
    {
        experiment.Use(() => IsCollaborator(user));
        experiment.Try(() => HasAccess(user));
    });
}
```

You can also specify a custom comparator.

```csharp
public IUser GetCurrentUser(string hash)
{
    return Scientist.Science<IUser>("experiment-name", experiment =>
    {
        experiment.Compare((x, y) => x.Name == y.Name);

        experiment.Use(() => LookupUser(hash));
        experiment.Try(() => RetrieveUser(hash));
    });
}
```

You can also limit the experiments from running.

```csharp
public decimal GetUserStatistic(IUser user)
{
    return Scientist.Science<decimal>("NewStatisticCalculation", experiment =>
    {
        experiment.RunIf(() => user.IsTestSubject);

        experiment.Use(() => CalculateStatistic(user));
        experiment.Try(() => NewCalculateStatistic(user));
    });
}
```

Control whether experiments run on a global level.

```csharp
Scientist.Enabled(() => ...);
```

To ensure that experimental results always match use `ThrownOnMismatches`.

```csharp
Scientist.Science<int>("ExperimentN", experiment => 
{
    experiment.ThrowOnMismatches = true;
    // ...
});
```

Use `Thrown` in order to track and manage any exceptions thrown during the life cycle of an experiment.  By default `Scientist` will throw all exceptions.

```csharp
Scientist.Scient<int>("ExperimentCatch", experiment =>
{
    experiment.Thrown((operation, exception) => InternalTracker.Track($"Science failure in ExperimentCatch: {operation}.", exception))
    // ...
});
```

By default observations are stored in an in-memory publisher. For production use, you'll
probably want to implement an `IResultPublisher`.

To give it a twirl, use NuGet to install it.

`Install-Package Scientist -Pre`