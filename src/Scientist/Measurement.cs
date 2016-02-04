using System;

namespace GitHub
{
    public class Measurement
    {
        public Measurement(string name, bool success, TimeSpan controlDuration, TimeSpan candidateDuration)
        {
        }

        public bool Success { get; }
        public TimeSpan Duration { get; }
        public string Name { get; }
     }
}
