using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scientist.Internals;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                Dostuff();
               int x = Enumerable.Range(0, 1000).Sum(s=> int.Parse(s.ToString()));
                Thread.Sleep(x/x);
                Console.WriteLine("----------------------");
            }
            Console.ReadKey();
        }

        static void Dostuff()
        {
            //Arrange
            Chrono chrono1;
            Chrono chrono2;
            Chrono chrono3;
            //Act

            //using (new Chronometer(out chrono1))
            //{
            //    //Thread.Sleep(10);
            //}
            //using (new Chronometer(out chrono2))
            //{
            //    //Thread.Sleep(10);
            //}
            //using (new Chronometer(out chrono3))
            //{
            //    Thread.Sleep(10);
            //}

            using ( Chronometer.New(out chrono1))
            {
                //Thread.Sleep(10);
            }
            using (Chronometer.New(out chrono2))
            {
                //Thread.Sleep(10);
            }
            using (Chronometer.New(out chrono3))
            {
                Thread.Sleep(10);
            }



            //Assert

            Console.WriteLine(chrono1.Timespan.Ticks);

            Console.WriteLine(chrono2.Timespan.Ticks);

            Console.WriteLine(chrono3.Timespan.Ticks);
        }
    }
}
