using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using UnitTests;
using Xunit;

using System.Reactive.Linq;

public class TheScientistClass
{
    public class TheScienceMethod
    {
        [Fact]
        public async void RunsBothBranchesOfTheExperimentAndReportsSuccess()
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

            var observation = await TestHelper.ObservationsGeneratedInThisMethod().FirstAsync();
            Assert.True(observation.Success);
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

            var observation = await TestHelper.ObservationsGeneratedInThisMethod().FirstAsync();
            Assert.False(observation.Success);
        }

        [Fact]
        public async void AllowsReturningNullFromControlOrTest()
        {
            var result = Scientist.Science<object>("failure", experiment =>
                {
                    experiment.Use(() => null);
                    experiment.Try(() => null);
                });

            Assert.Null(result);

            var observation = await TestHelper.ObservationsGeneratedInThisMethod().FirstAsync();

            Assert.True(observation.Success);
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
        public async void RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations()
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

            var observation = await TestHelper.ObservationsGeneratedInThisMethod().FirstAsync();
            Assert.True(observation.Success);
            Assert.True(observation.ControlDuration.Ticks > 0);
            Assert.True(observation.CandidateDuration.Ticks > 0);
        }

        [Fact]
        public async void AnExceptionReportsDuration()
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

            var observation = await TestHelper.ObservationsGeneratedInThisMethod().FirstAsync();
            Assert.True(observation.Success == false);
            Assert.True(observation.ControlDuration.Ticks > 0);
            Assert.True(observation.CandidateDuration.Ticks > 0);
        }
    }
}
