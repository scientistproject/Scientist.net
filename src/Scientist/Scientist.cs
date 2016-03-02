using GitHub.Internals;
using System;
using System.Threading.Tasks;

namespace GitHub
{
    /// <summary>
    /// A class for carefully refactoring critical paths. Use <see cref="Scientist"/> 
    /// </summary>
    public static class Scientist
    {
        // Should be configured once before starting observations.
        // TODO: How can we guide the developer to the pit of success
        public static IResultPublisher ResultPublisher
        {
            get;
            set;
        } = new InMemoryResultPublisher();

        /// <summary>
        /// Conduct a synchronous experiment
        /// </summary>
        /// <typeparam name="T">The return type of the experiment</typeparam>
        /// <param name="name">Name of the experiment</param>
        /// <param name="experiment">Experiment callback used to configure the experiment</param>
        /// <returns>The value of the experiment's control function.</returns>
#if NET451
        [return: AllowNull]
#endif
        public static Experiment<T> Science<T>(string name)
        {
            // TODO: Maybe we could automatically generate the name if none is provided using the calling method name. We'd have to 
            // make sure we don't inline this method though.
            return new Experiment<T>(name);
        }
    }
}
