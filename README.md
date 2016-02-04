# Scientist.NET

This is a .NET Port of the Ruby Scientist library. Read [about it here](http://githubengineering.com/scientist/).

Currently, it's a rough sketch of what the library will look like.

Here's a sample usage.


```csharp
public bool MayPush(IUser user)
{
  var result = await Scientist.ScienceAsync<int>("experiment-name", experiment =>
  {
      experiment.Use(() => IsCollaborator(user));
      experiment.Try(() => HasAccess(user));
  });
}

```

By default measurements are stored in an in-memory publisher. For production use, you'll
probably want to implement an `IMeasurementPublisher`.