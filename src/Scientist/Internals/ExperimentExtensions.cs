using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    public static class ExperimentExtensions
    {
        public static Experiment<T> Where<T>(this Experiment<T> experiment, Func<Task<bool>> block)
        {
            experiment.RunIf = block;
            return experiment;
        }

        public static Experiment<T> Where<T>(this Experiment<T> experiment, Func<bool> block)
        {
            experiment.RunIf = () => Task.FromResult(block());
            return experiment;
        }

        public static Experiment<T> Use<T>(this Experiment<T> experiment, Func<Task<T>> control)
        {
            experiment.Control = control;
            return experiment;
        }

        public static Experiment<T> Use<T>(this Experiment<T> experiment, Func<T> control)
        {
            experiment.Control = () => Task.FromResult(control());
            return experiment;
        }
        
        // TODO add optional name parameter, and store all delegates into a dictionary.
        public static Experiment<T> Try<T>(this Experiment<T> experiment, Func<Task<T>> candidate)
        {
            experiment.Candidate = candidate;
            return experiment;
        }
        public static Experiment<T> Try<T>(this Experiment<T> experiment, Func<T> candidate)
        {
            experiment.Candidate = () => Task.FromResult(candidate());
            return experiment;
        }

        public static Experiment<T> WithComparer<T>(this Experiment<T> experiment, Func<T, T, bool> comparison)
        {
            experiment.Comparison = comparison;
            return experiment;
        }
        
        public static Experiment<T> BeforeRun<T>(this Experiment<T> experiment, Func<Task> action)
        {
            experiment.BeforeRun = action;
            return experiment;
        }

        public static Experiment<T> BeforeRun<T>(this Experiment<T> experiment, Action action)
        {
            experiment.BeforeRun = () => Task.Run(action);
            return experiment;
        }

        public static T Execute<T>(this Experiment<T> experiment)
        {
            return experiment.Build().RunAsync().Result;
        }

        public static async Task<T> ExecuteAsync<T>(this Experiment<T> experiment)
        {
            return await experiment.Build().RunAsync();
        }
    }
}