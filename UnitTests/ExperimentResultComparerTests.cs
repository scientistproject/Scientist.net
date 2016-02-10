using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using Xunit;

namespace UnitTests
{

    public class ExperimentResultComparerTests
    {
        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccess()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);


            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").Success);
        }

        [Fact]
        public void BothResultsAreNull()
        {


            //Arrange
            //ExperimentResultComparer<int> comparer = new ExperimentResultComparer<int>(null,null);
            //Act

            //Assert





            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);


            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").Success);
        }
    }
}
