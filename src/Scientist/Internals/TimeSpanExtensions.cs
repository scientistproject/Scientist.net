namespace GitHub.Internals
{
    using System;

    public static class TimeSpanExtensions
    {
        public const long TicksPerNanosecond  = 1;

        public const long TicksPerMicrosecond = 1000;

        public static string ToNanoSecondsString(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Ticks / TicksPerNanosecond}" + " ns";
        }

        public static string ToMicroSecondString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TicksPerMicrosecond:0.00}" + " us";
        }

        public static string ToMilliSecondString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TimeSpan.TicksPerMillisecond:0.00}" + " ms";
        }

        public static string ToSecondString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TimeSpan.TicksPerSecond:0.00}" + " s";
        }

        public static string ToMinuteString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TimeSpan.TicksPerMinute:0.00}" + " m";
        }

        public static string ToHourString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TimeSpan.TicksPerHour:0.00}" + " h";
        }

        public static string ToDayString(this TimeSpan timeSpan)
        {
            return $"{(double)timeSpan.Ticks / TimeSpan.TicksPerDay:0.00}" + " d";
        }

        public static string ToScaledString(this TimeSpan timeSpan)
        {
            if (timeSpan.Ticks < TicksPerMicrosecond)
            {
                return timeSpan.ToNanoSecondsString();
            }
            else if (timeSpan.Ticks < TimeSpan.TicksPerMillisecond)
            {
                return timeSpan.ToMicroSecondString();
            }
            else if (timeSpan.Ticks < TimeSpan.TicksPerSecond)
            {
                return timeSpan.ToMilliSecondString();
            }
             else if (timeSpan.Ticks < TimeSpan.TicksPerMinute)
            {
                return timeSpan.ToSecondString();
            }
            else if (timeSpan.Ticks < TimeSpan.TicksPerHour)
            {
                return timeSpan.ToMinuteString();
            }
            else if (timeSpan.Ticks < TimeSpan.TicksPerDay)
            {
                return timeSpan.ToHourString();
            }
            else
            {
                return timeSpan.ToDayString();
            }
        }
    }
}