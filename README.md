# Scientist.NET

A .NET Port of the [Scientist](https://github.com/github/scientist) library for carefully refactoring critical paths. 

[![Build status](https://ci.appveyor.com/api/projects/status/b548cd5okkel3h4x/branch/master?svg=true)](https://ci.appveyor.com/project/shiftkey/scientist-net/branch/master)
[![Gitter](https://badges.gitter.im/scientistproject/community.svg)](https://gitter.im/scientistproject/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

To give it a twirl, use NuGet to install: `Install-Package Scientist`

## How do I science?

Let's pretend you're changing the way you handle permissions in a large web app. Tests can help guide your refactoring, but you really want to compare the current and refactored behaviors under load.

```csharp
using GitHub;

...

public bool CanAccess(IUser user)
{
    return Scientist.Science<bool>("widget-permissions", experiment =>
    {
        experiment.Use(() => IsCollaborator(user)); // old way
        experiment.Try(() => HasAccess(user)); // new way
    }); // returns the control value
}
```

Wrap a `Use` block around the code's original behavior, and wrap `Try` around the new behavior. Invoking `Scientist.Science<T>` will always return whatever the `Use` block returns, but it does a bunch of stuff behind the scenes:

* It decides whether or not to run the `Try` block,
* Randomizes the order in which `Use` and `Try` blocks are run,
* Measures the durations of all behaviors,
* Compares the result of `Try` to the result of `Use`,
* Swallows (but records) any exceptions raised in the `Try` block, and
* Publishes all this information.

The `Use` block is called the **control**. The `Try` block is called the **candidate**.

If you don't declare any `Try` blocks, none of the Scientist machinery is invoked and the control value is always returned.

## Making science useful

### Publishing results

What good is science if you can't publish your results?

By default results are published in an in-memory publisher. To override this behavior, create your own implementation of `IResultPublisher`:

```csharp
public class MyResultPublisher : IResultPublisher
{
    public Task Publish<T, TClean>(Result<T, TClean> result)
    {
        Logger.Debug($"Publishing results for experiment '{result.ExperimentName}'");
        Logger.Debug($"Result: {(result.Matched ? "MATCH" : "MISMATCH")}");
        Logger.Debug($"Control value: {result.Control.Value}");
        Logger.Debug($"Control duration: {result.Control.Duration}");
        foreach (var observation in result.Candidates)
        {
            Logger.Debug($"Candidate name: {observation.Name}");
            Logger.Debug($"Candidate value: {observation.Value}");
            Logger.Debug($"Candidate duration: {observation.Duration}");
        }

        if (result.Mismatched)
        {
            // saved mismatched experiments to DB
            DbHelpers.SaveExperimentResults(result);
        }

        return Task.FromResult(0);
    }
}
```

Then set Scientist to use it before running the experiments:

```csharp
Scientist.ResultPublisher = new MyResultPublisher();
```

As of v1.0.2, A `IResultPublisher` can also be wrapped in `FireAndForgetResultPublisher` so that result publishing avoids any delays in running experiments and is delegated to another thread:

```csharp
Scientist.ResultPublisher = new FireAndForgetResultPublisher(new MyResultPublisher(onPublisherException));
```

### Controlling comparison

Scientist compares control and candidate values using `==`. To override this behavior, use `Compare` to define how to compare observed values instead:

```csharp
public IUser GetCurrentUser(string hash)
{
    return Scientist.Science<IUser>("get-current-user", experiment =>
    {
        experiment.Compare((x, y) => x.Name == y.Name);

        experiment.Use(() => LookupUser(hash));
        experiment.Try(() => RetrieveUser(hash));
    });
}
```

### Adding context


Results aren't very useful without some way to identify them. Use the `AddContext` method to add to the context for an experiment:

```csharp
public IUser GetUserByName(string userName)
{
    return Scientist.Science<IUser>("get-user-by-name", experiment =>
    {
        experiment.AddContext("username", userName);

        experiment.Use(() => FindUser(userName));
        experiment.Try(() => GetUser(userName));
    });
}
```

`AddContext` takes a string identifier and an object value, and adds them to an internal `Dictionary`. When you publish the results, you can access the context by using the ```Contexts``` property:

```csharp
public class MyResultPublisher : IResultPublisher
{
    public Task Publish<T, TClean>(Result<T, TClean> result)
    {
        foreach (var kvp in result.Contexts)
        {
            Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
        return Task.FromResult(0);
    }
}
```

### Expensive setup

If an experiment requires expensive setup that should only occur when the experiment is going to be run, define it with the `BeforeRun` method:

```csharp
public int DoSomethingExpensive()
{
    return Scientist.Science<int>("expensive-but-worthwile", experiment =>
    {
        experiment.BeforeRun(() => ExpensiveSetup());

        experiment.Use(() => TheOldWay());
        experiment.Try(() => TheNewWay());
    });
}
```

### Keeping it clean

Sometimes you don't want to store the full value for later analysis. For example, an experiment may return `IUser` instances, but when researching a mismatch, all you care about is the logins. You can define how to clean these values in an experiment:

```csharp
public IUser GetUserByEmail(string emailAddress)
{
    return Scientist.Science<IUser, string>("get-user-by-email", experiment =>
    {
        experiment.Use(() => OldApi.FindUserByEmail(emailAddress));
        experiment.Try(() => NewApi.GetUserByEmail(emailAddress));
        
        experiment.Clean(user => user.Login);
    });
}
```

And this cleaned value is available in the final published result:

```csharp
public class MyResultPublisher : IResultPublisher
{
    public Task Publish<T, TClean>(Result<T, TClean> result)
    {
        // result.Control.Value = <IUser object>
        IUser user = (IUser)result.Control.Value;
        Console.WriteLine($"Login from raw object: {user.Login}");
        
        // result.Control.CleanedValue = "user name"
        Console.WriteLine($"Login from cleaned object: {result.Control.CleanedValue}");
        
        return Task.FromResult(0);
    }
}
```

### Ignoring mismatches

During the early stages of an experiment, it's possible that some of your code will always generate a mismatch for reasons you know and understand but haven't yet fixed. Instead of these known cases always showing up as mismatches in your metrics or analysis, you can tell an experiment whether or not to ignore a mismatch using the `Ignore` method. You may include more than one block if needed:

```csharp
public bool CanAccess(IUser user)
{
    return Scientist.Science<bool>("widget-permissions", experiment =>
    {
        experiment.Use(() => IsCollaborator(user));
        experiment.Try(() => HasAccess(user));

        // user is staff, always an admin in the new system
        experiment.Ignore((control, candidate) => user.IsStaff);
        // new system doesn't handle unconfirmed users yet
        experiment.Ignore((control, candidate) => control && !candidate && !user.ConfirmedEmail);
    });
}
```

The ignore blocks are only called if the *values* don't match. If one observation raises an exception and the other doesn't, it's always considered a mismatch. If both observations raise different exceptions, that is also considered a mismatch.

### Enabling/disabling experiments

Sometimes you don't want an experiment to run. Say, disabling a new codepath for anyone who isn't staff. You can disable an experiment by setting a `RunIf` block. If this returns `false`, the experiment will merely return the control value. Otherwise, it defers to the global `Scientist.Enabled` method.

```csharp
public decimal GetUserStatistic(IUser user)
{
    return Scientist.Science<decimal>("new-statistic-calculation", experiment =>
    {
        experiment.RunIf(() => user.IsTestSubject);

        experiment.Use(() => CalculateStatistic(user));
        experiment.Try(() => NewCalculateStatistic(user));
    });
}
```

### Ramping up experiments

As a scientist, you know it's always important to be able to turn your experiment off, lest it run amok and result in villagers with pitchforks on your doorstep. You can set a global switch to control whether or not experiments is enabled by using the `Scientist.Enabled` method.

```csharp
int percentEnabled = 10;
Random rand = new Random();
Scientist.Enabled(() =>
{
    return rand.Next(100) < percentEnabled;
});
```

This code will be invoked for every method with an experiment every time, so be sensitive about its performance. For example, you can store an experiment in the database but wrap it in various levels of caching.

### Running candidates in parallel (asynchronous)

Scientist runs tasks synchronously by default. This can end up doubling (more or less) the time it takes the original method call to complete, depending on how many candidates are added and how long they take to run.

In cases where Scientist is used for production refactoring, for example, this ends up causing the calling method to return slower than before which may affect the performance of your original code. However, if the candidates can be run at the same time as the control method without affecting each other, then they can be run in parallel so the Scientist call will only take as long as the slowest task (plus a tiny bit of overhead):

```csharp
await Scientist.ScienceAsync<int>(
	"ExperimentName",
	3, // number of tasks to run concurrently 
	experiment => {
        experiment.Use(async () => await StartRunningSomething(myData));
        experiment.Try(async () => await RunAtTheSameTimeAsTheControlMethod(myData));
        experiment.Try(async () => await AlsoRunThisConcurrently(myData));
	});
```

As always when using async/await, don't forget to call `.ConfigureAwait(false)` where appropriate.

### Testing

When running your test suite, it's helpful to know that the experimental results always match. To help with testing, Scientist has a `ThrowOnMismatches` property that can be set to `true`. Only do this in your test suite!

To throw on mismatches:

```csharp
Scientist.Science<int>("ExperimentN", experiment => 
{
    experiment.ThrowOnMismatches = true;
    // ...
});
```

Scientist will throw a `MismatchException<T, TClean>` exception if any observations don't match.

### Handling errors

If an exception is thrown in any of Scientist's internal helpers like `Compare`, `Enabled`, or `Ignore`, the default behavior of Scientist is to re-throw that exception. Since this halts the experiment entirely, it's often a better idea to handle this error and continue so the experiment as a whole isn't canceled entirely:

```csharp
Scientist.Science<int>("ExperimentCatch", experiment =>
{
    experiment.Thrown((operation, exception) => InternalTracker.Track($"Science failure in ExperimentCatch: {operation}.", exception))
    // ...
});
```

The operations that may be handled here are:

* `Operation.Compare` - an exception is raised in a `Compare` block
* `Operation.Enabled` - an exception is raised in the `Enabled` block
* `Operation.Ignore` - an exception is raised in an `Ignore` block
* `Operation.Publish` - an exception is raised while publishing results
* `Operation.RunIf` - an exception is raised in a `RunIf` block

### Designing an experiment

Because `Enabled` and `RunIf` determine when a candidate runs, it's impossible to guarantee that it will run every time. For this reason, Scientist is only safe for wrapping methods that aren't changing data.

When using Scientist, we've found it most useful to modify both the existing and new systems simultaneously anywhere writes happen, and verify the results at read time with `Science`. `ThrowOnMismatches` has also been useful to ensure that the correct data was written during tests, and reviewing published mismatches has helped us find any situations we overlooked with our production data at runtime. When writing to and reading from two systems, it's also useful to write some data reconciliation scripts to verify and clean up production data alongside any running experiments.

### Finishing an experiment

As your candidate behavior converges on the controls, you'll start thinking about removing an experiment and using the new behavior.

* If there are any ignore blocks, the candidate behavior is *guaranteed* to be different. If this is unacceptable, you'll need to remove the ignore blocks and resolve any ongoing mismatches in behavior until the observations match perfectly every time.
* When removing a read-behavior experiment, it's a good idea to keep any write-side duplication between an old and new system in place until well after the new behavior has been in production, in case you need to roll back.

## Breaking the rules

Sometimes scientists just gotta do weird stuff. We understand.


### Ignoring results entirely

Science is useful even when all you care about is the timing data or even whether or not a new code path blew up. If you have the ability to incrementally control how often an experiment runs via your `Enabled` method, you can use it to silently and carefully test new code paths and ignore the results altogether. You can do this by setting `Ignore((x, y) => true)`, or for greater efficiency, `Compare((x, y) => true)`.

This will still log mismatches if any exceptions are raised, but will disregard the values entirely.

### Trying more than one thing

It's not usually a good idea to try more than one alternative simultaneously. Behavior isn't guaranteed to be isolated and reporting + visualization get quite a bit harder. Still, it's sometimes useful.

To try more than one alternative at once, add names to some `Try` blocks:

```csharp
public bool CanAccess(IUser user)
{
    return Scientist.Science<bool>("widget-permissions", experiment =>
    {
        experiment.Use(() => IsCollaborator(user));
        experiment.Try("api", () => HasAccess(user));
        experiment.Try("raw-sql", () => HasAccessSql(user));
    });
}
```

## Alternatives

Here are other implementations of Scientist available in different languages.

- [github/scientist](https://github.com/github/scientist) the original implementation of Scientist, in [Ruby](https://www.ruby-lang.org/).
- [daylerees/scientist](https://github.com/daylerees/scientist) for [PHP](http://php.net/) by [Dayle Rees](https://github.com/daylerees).
