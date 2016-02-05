using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using Moq;
using UnitTests;
using Xunit;

public class TheScientistClass
{
    public class TheScienceMethod
    {
        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccess()
        {
            Mock<IControlCandidate<int> > mock = new Mock<IControlCandidate<int>>();
            mock.Setup(s => s.Control()).Returns(42);
            mock.Setup(s => s.Candidate()).Returns(42);
            var fake = mock.Object;

            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(fake.Control);
                experiment.Try(fake.Candidate);
            });

            Assert.Equal(42, result);
            mock.Verify(x => x.Control(), Times.Once);
            mock.Verify(x => x.Candidate(), Times.Once);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").Success);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailure()
        {
            var mock = new Mock<IControlCandidateTask<int>>();
            mock.Setup(s => s.Control()).Returns(Task.FromResult(42));
            mock.Setup(s => s.Candidate()).Returns(Task.FromResult(43));
            var fake = mock.Object;



            var result = await Scientist.ScienceAsync<int>("failure", experiment =>
            {
                experiment.Use(fake.Control);
                experiment.Try(fake.Candidate);
            });

            Assert.Equal(42, result);
            mock.Verify(x => x.Control(), Times.Once);
            mock.Verify(x => x.Candidate(), Times.Once);
            Assert.False(TestHelper.Observation.First(m => m.Name == "failure").Success);
        }

        [Fact]
        public void AllowsReturningNullFromControlOrTest()
        {
            var result = Scientist.Science<object>("failure", experiment =>
            {
                experiment.Use(() => null);
                experiment.Try(() => null);
            });

            Assert.Null(result);
            Assert.True(TestHelper.Observation.First(m => m.Name == "failure").Success);
        }

        [Fact]
        public void EnsureNullGuardIsWorking()
        {
#if !DEBUG
            Assert.Throws<ArgumentNullException>(() =>
                Scientist.Science<object>(null, _ => { })
            );
#endif
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations()
        {
            Mock<IControlCandidate<int>> mock = new Mock<IControlCandidate<int>>();
            mock.Setup(s => s.Control()).Returns(42);
            mock.Setup(s => s.Candidate()).Returns(42);
            var fake = mock.Object;


            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(fake.Control);
                experiment.Try(fake.Candidate);
            });

            Assert.Equal(42, result);
            mock.Verify(x => x.Control(), Times.Once);
            mock.Verify(x => x.Candidate(), Times.Once);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").Success);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").ControlDuration.Ticks > 0);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.Name == "success").CandidateDuration.Ticks > 0);
        }
    }
}
