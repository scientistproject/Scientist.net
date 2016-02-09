using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using GitHub;
using GitHub.Internals;
using Github.Internals;

public class TimerTests
{
    public class TheChronometer
    {
        [Fact]
        public void Chronometer_can_used_in_a_using_block__measurement_overhead_is_low()
        {
            //Arrange
            Chrono chrono1;
            Chrono chrono2;
            Chrono chrono3;
            //Act

            using (Chronometer.New(out chrono1))
            {
                //nothing
            }
            using (Chronometer.New(out chrono2))
            {
                //nothing
            }
            using (Chronometer.New(out chrono3))
            {
                Thread.Sleep(10);//Sleep will take *at least* 10ms
            }

            Console.WriteLine($"Time taken: {chrono3.Timespan.Ticks} (in Ticks)");
            //Assert

            Assert.InRange(chrono1.Timespan.Ticks, 0, 50);              //first run takes less than 50 ticks
            Assert.InRange(chrono2.Timespan.Ticks, 0, 50);              //second run also takes less than 50 ticks
            Assert.InRange(chrono3.Timespan.Ticks, 100000, 1000000);     //Actually measures something (Thread.Sleep(10) >= 10ms, 10ms == 100000 Ticks)

        }
    }
}

