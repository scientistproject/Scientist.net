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
public bool GetCurrentUser(string hash)
{
    return Scientist.Science<IUser>("experiment-name", experiment =>
    {
        experiment.Use(() => LookupUser(hash));
        experiment.Try(() => RetrieveUser(hash));
		experiment.Compare((x, y) => x.Name == y.Name);
    });
}
```

By default observations are stored in an in-memory publisher. For production use, you'll
probably want to implement an `IResultPublisher`.

To give it a twirl, use NuGet to install it.

`Install-Package Scientist -Pre`