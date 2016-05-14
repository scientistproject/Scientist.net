using GitHub;
using System;

/// <summary>
/// Class that swaps the value with a temporary item,
/// and replaces it upon disposal with the original.
/// </summary>
/// <typeparam name="T">The type to switch.</typeparam>
public class Swap<T> : IDisposable
{
    readonly T _original;
    readonly Action<T> _set;

    public Swap(T temporary, Func<T> get, Action<T> set)
    {
        _original = get();
        _set = set;
        set(temporary);
    }

    public void Dispose() => _set(_original);
}

public static class Swap
{
    /// <summary>
    /// Swaps <see cref="Scientist.Laboratory"/> with the input
    /// parameter, and upon disposal exchanges the laboratory back.
    /// </summary>
    /// <param name="laboratory">The laboratory to swap temporarily.</param>
    /// <returns>A new <see cref="Swap{ILaboratory}"/> instance.</returns>
    public static IDisposable Laboratory(ILaboratory laboratory) =>
        new Swap<ILaboratory>(laboratory, () => Scientist.Laboratory, (lab) => Scientist.Laboratory = lab);

    /// <summary>
    /// Swaps <see cref="Scientist.ResultPublisher"/> with the input
    /// parameter, and upon disposal exchanges the publisher back.
    /// </summary>
    /// <param name="publisher">The publisher to swap temporarily.</param>
    /// <returns>A new <see cref="Swap{IResultPublisher}"/> instance.</returns>
    public static IDisposable Publisher(IResultPublisher publisher) =>
        new Swap<IResultPublisher>(publisher, () => Scientist.ResultPublisher, (pub) => Scientist.ResultPublisher = pub);
}
