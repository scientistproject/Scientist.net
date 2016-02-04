# Scientist.NET

This is a .NET Port of the Ruby Scientist library. Read [about it here](http://githubengineering.com/scientist/).
It's really really early. We'll be moving this to the GitHub org on GitHub.com once it's been battle tested.
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

The real power of Scientist lies in publishing measurements to a data store so 
you can run stats against it. Right now, there's only a dirt simple in memory
data store. We'll be very interested in having contributions to store measurements
in SQL Server, Redis, etc. So help us out!