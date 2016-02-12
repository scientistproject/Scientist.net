using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using UnitTests;
using Xunit;

public class TheScientistClass
{
    public class TheScienceMethod
    {
        [Fact]
        public void RunsBothBranchesOfTheExperimentAndMatchesExceptions()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => { throw new InvalidOperationException(); });
            mock.Candidate().Returns(x => { throw new InvalidOperationException(); });
            const string experimentName = "successException";

            var ex = Assert.Throws<AggregateException>(() =>
            {
                Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<InvalidOperationException>(baseException);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccess()
        {
            var mock = Substitute.For< IControlCandidate<int>>();
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
            Assert.True(((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == "success").Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailure()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(Task.FromResult(43));




            var result = await Scientist.ScienceAsync<int>("failure", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(TestHelper.Results.First(m => m.ExperimentName == "failure").Matched);
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
            Assert.True(TestHelper.Results.First(m => m.ExperimentName == "failure").Matched);
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
            Assert.True(((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == "success").Matched);
            Assert.True(((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == "success").Control.Duration.Ticks > 0);
            Assert.True(((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == "success").Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void AnExceptionReportsDuration()
        {
            var candidateRan = false;
            var controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            // Do we do a deep comparison?
            Func<int> control = () => { controlRan = true; return 42; };
            Func<int> candidate = () => { candidateRan = true; throw new InvalidOperationException(); };

            var result = Scientist.Science<int>("failure", experiment =>
            {
                experiment.Use(control);
                experiment.Try(candidate);
            });

            Assert.Equal(42, result);
            Assert.True(candidateRan);
            Assert.True(controlRan);
            IResult observedResult = ((InMemoryResultPublisher)Scientist.ResultPublisher).Results.First(m => m.ExperimentName == "success");
            Assert.True(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }
    }
}
