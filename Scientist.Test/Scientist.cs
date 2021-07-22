using System;
using Xunit;

namespace Scientist.Test
{
    public class ScientistTest
    {
        [Fact]
        public void IsEven_EvenNumber_ExpectTrue()
        {
            const int testData = 42;

            var isEven = Scientist.IsEven(testData);

            Assert.True(isEven);
        }
    }
}
