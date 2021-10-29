using System;
using System.Threading.Tasks;
using GitHub;

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
    /// Swaps <see cref="Scientist"/> enabled value with the input
    /// parameter, and upon disposal exchanges the enabled back.
    /// </summary>
    /// <param name="enabled">The delegate to swap temporarily.</param>
    /// <returns>A new <see cref="Swap{Func{Task{bool}}}"/> instance.</returns>
    public static IDisposable Enabled(Func<Task<bool>> enabled) =>
        new Swap<Func<Task<bool>>>(enabled, () => () => Task.FromResult(true), (del) => Scientist.Enabled(del));

    /// <summary>
    /// Swaps <see cref="Scientist"/> enabled control value with the input
    /// parameter, and upon disposal exchanges the enabled back.
    /// </summary>
    /// <param name="enabled">The delegate to swap temporarily.</param>
    /// <returns>A new <see cref="Swap{Func{Task{bool}}}"/> instance.</returns>
    public static IDisposable EnableControl(Func<Task<bool>> enabled) =>
        new Swap<Func<Task<bool>>>(enabled, () => () => Task.FromResult(true), (del) => Scientist.EnabledControl(del));
}
