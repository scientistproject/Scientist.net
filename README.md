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

Provide a global condition that determines whether experiments run or not.

```csharp
int peakStart = 12; // 12:00
int peakEnd = 15;   // 15:00

Scientist.Enabled(() => DateTime.UtcNow.Hour < peakStart || DateTime.UtcNow.Hour > peakEnd);
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
Scientist.Science<int>("ExperimentCatch", experiment =>
{
    experiment.Thrown((operation, exception) => InternalTracker.Track($"Science failure in ExperimentCatch: {operation}.", exception))
    // ...
});
```

When publishing result observations the complete value may not be necessary.  In this example `Clean` is used to return simply `IUser.Name` instead of the full value.  This allows the caller to keep the return of `IUser`, and the publisher to use `string`.

```csharp
public IUser GetCurrentUser(string hash)
{
    return Scientist.Science<IUser, string>("experiment-name", experiment =>
    {
        experiment.Use(() => LookupUser(hash));
        experiment.Try(() => RetrieveUser(hash));
        experiment.Clean(user => user.Name);
    });
}
```

Here is how to access the two different values during the publish routine.

```csharp
public Task Publish<T, TClean>(Result<T, TClean> result)
{
    result.Control.Value        // {IUser}
    result.Control.CleanedValue // "Jonathan"
}
```

If you need to ensure that the scientist results are published prior to your program's exit use `Scientist.WhenPublished()`:+1:

```csharp
static async void MainAsync(string[] args)
{
    await RunProgram(args);

    await Scientist.WhenPublished();
}
```

By default observations are stored in an in-memory publisher. For production use, you'll
probably want to implement an `IResultPublisher`.

To give it a twirl, use NuGet to install it.

`Install-Package Scientist -Pre`