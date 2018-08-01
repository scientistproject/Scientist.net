namespace GitHub.Internals
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class GCHelper
    {
        private const string DebugConfigurationName = "DEBUG";
        internal const string ReleaseConfigurationName = "RELEASE";
        internal const string Unknown = "?";

        public static bool IsMono { get; } = Type.GetType("Mono.Runtime") != null;

        public static bool IsFullFramework =>
#if (NET451)
            true;
#else
            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
#endif

        public static bool IsNetCore =>
#if (NET451)
            false;
#else
            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);
#endif

        private static long AllocationQuantum { get; } = IsNetCore ? 0 : CalculateAllocationQuantumSize();

        private static readonly Func<long> GetCurrentAllocatedBytesDelegate = GetAllocatedBytesDelegate();

        /// <summary>
        /// returns total allocated bytes (not per operation)
        /// </summary>
        /// <param name="excludeAllocationQuantumSideEffects">Allocation quantum can affecting some of our nano-benchmarks in non-deterministic way.
        /// when this parameter is set to true and the number of all allocated bytes is less or equal AQ, we ignore AQ and put 0 to the results</param>
        /// <returns></returns>
        public static long GetAllocatedBytes()
        {
            return GetCurrentAllocatedBytesDelegate();
        }

        private static Func<long> GetAllocatedBytesDelegate()
        {
            // Monitoring is not available in Mono, see http://stackoverflow.com/questions/40234948/how-to-get-the-number-of-allocated-bytes-
            if (IsMono)
            {
                return () => 0L;
            }
#if (NET451)

            return () => {
                GC.Collect();
                var allocation = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
                return allocation <= AllocationQuantum ? 0L : allocation;
            };

#elif (NETSTANDARD2_0)
            // While part of the standard, AppDomain is NotImplemented in .Net Core
            if (IsFullFramework)
            {
                return () =>
                {
                    GC.Collect();
                    var allocation = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
                    return allocation <= AllocationQuantum ? 0L : allocation;
                };
            }
            else
            {
                // for some versions of .NET Core this method is internal, 
                // for some public and for others public and exposed ;)
                var method = typeof(GC)
                                .GetTypeInfo()
                                .GetMethod("GetAllocatedBytesForCurrentThread",
                                BindingFlags.Public | BindingFlags.Static)
                             ?? typeof(GC)
                                .GetTypeInfo()
                                .GetMethod("GetAllocatedBytesForCurrentThread",
                                BindingFlags.NonPublic | BindingFlags.Static);
                return (Func<long>) method.CreateDelegate(typeof(Func<long>));
            }
#elif (NETCOREAPP2_1)
            // but CoreRT does not support the reflection yet, so only because of that we have to target .NET Core 2.1
            // to be able to call this method without reflection and get MemoryDiagnoser support for CoreRT ;)
            return GC.GetAllocatedBytesForCurrentThread();
#else
            return () => 0L;
#endif
        }

        /// <summary>
        /// code copied from https://github.com/rsdn/CodeJam/blob/71a6542b6e5c52ea8dd92c601adad11e62796a98/PerfTests/src/%5BL4_Configuration%5D/Metrics/%5BMetricValuesProvider%5D/GcMetricValuesProvider.cs#L63-L89
        /// </summary>
        /// <returns></returns>
        private static long CalculateAllocationQuantumSize()
        {
            long result;
            int retry = 0;
            do
            {
                if (++retry > 10)
                {
                    // 8kb https://github.com/dotnet/coreclr/blob/master/Documentation/botr/garbage-collection.md
                    result = 8192;
                    break;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                result = GC.GetTotalMemory(false);
                var tmp = new object();
                result = GC.GetTotalMemory(false) - result;
                GC.KeepAlive(tmp);
            } while (result <= 0);

            return result;
        }
    }
}