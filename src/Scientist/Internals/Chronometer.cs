using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Github.Internals
{
    /// <summary>
    /// General purpose timing helper object.
    /// Should be used in a using block.
    /// <example>
    /// Chrono chrono;
    /// using ( Chronometer.New(out chrono))
    /// {
    ///       //code to test execution time
    /// }
    /// Console.WriteLine($"Time taken: {chrono.Timespan.Ticks} (in Ticks)");
    /// </example>
    /// </summary>
    internal class Chronometer : IDisposable
    {
        public Stopwatch Stopwatch { get; } = new Stopwatch();

        public Chrono ElapsedTime { get; } = new Chrono();

        private static bool _warmedUp;

        private Chronometer(out Chrono timespan)
        {
            timespan = ElapsedTime;

            Warmup();
            Stopwatch.Start();
        }


        /// <summary>
        /// Simple clean method to create a Chronometer object, for timing the performance of a block of code.
        /// </summary>
        /// <param name="timespan">Chrono object, its timespan property will be updated with elapsed time, of the code inside a using block.</param>
        /// <returns>Chronometer object to be used inside a using block</returns>
        public static Chronometer New(out Chrono timespan)
        {
            return new Chronometer(out timespan);
        }

        /// <summary>
        /// Because of jitting, the first run takes longer.
        /// This method reduces the first timing overhead, from 1000s of ticks, to less than 20 ticks.
        /// </summary>
        private void Warmup()
        {
            

            //Try to pre jit methods used by this class
            if (!_warmedUp)
            {
                _warmedUp = true;

                List<Type> typesUsedInClass = new List<Type>()
                {
                    GetType(),//this
                    Stopwatch.GetType(),
                    ElapsedTime.GetType(),
                    ElapsedTime.Timespan.GetType()
                };

                BindingFlags allBindingFlags = (BindingFlags) (~0);

                typesUsedInClass
                    .SelectMany(s => s.GetMethods(allBindingFlags))
                    .ToList()
                    .ForEach(method => RuntimeHelpers.PrepareMethod(method.MethodHandle));

                //It's weird, but you totally need to do this (even though we tried pre jitting. You need both!)
                using (this)
                {
                    //Yup this should be empty
                }
            
            }

            Stopwatch.Reset();
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            ElapsedTime.Timespan = Stopwatch.Elapsed;
        }

    }

    /// <summary>
    /// Reference type wrapper of value type TimeSpan
    /// Needed so we can pass out object from Chronometer constructor 
    /// </summary>
    internal class Chrono
    {
        public TimeSpan Timespan { get; set; } = TimeSpan.Zero;
    }


}
