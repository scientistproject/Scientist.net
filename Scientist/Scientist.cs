using System;
using System.Threading.Tasks;

namespace Scientist
{
    public class Scientist : IScientist
    {
        public static bool IsEven(int num) => num % 2 == 0;
    }
}
