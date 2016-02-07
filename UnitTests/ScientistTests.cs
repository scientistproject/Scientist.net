using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using Xunit;

public class TheScientistClass
{
    public class TheScienceMethod
    {
        [Fact]
        public void DoesntRunCandidate()
        {
            bool candidateRan = false;
            bool controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            // Do we do a deep comparison?
            const int expectedResult = 42;
            Func<int> control = () => { controlRan = true; return expectedResult; };
            Func<int> candidate = () => { candidateRan = true; return expectedResult; };

            const string experimentName = "successRunIf";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.RunIf(() => false);
                experiment.Use(control);
                experiment.Try(candidate);
            });
            Assert.Equal(expectedResult, result);
            Assert.False(candidateRan);
            Assert.True(controlRan);
            Assert.False(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.Any(m => m.ExperimentName == experimentName));
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndMatchesExceptions()
        {
            bool candidateRan = false;
            bool controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            // Do we do a deep comparison?
            Func<int> control = () => { controlRan = true; throw new InvalidOperationException(); };
            Func<int> candidate = () => { candidateRan = true; throw new InvalidOperationException(); };

            const string experimentName = "successException";

            var ex = Assert.Throws<AggregateException>(() =>
            {
                Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(control);
                    experiment.Try(candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<InvalidOperationException>(baseException);
            Assert.True(candidateRan);
            Assert.True(controlRan);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccess()
        {
            bool candidateRan = false;
            bool controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            // Do we do a deep comparison?
            Func<int> control = () => { controlRan = true; return 42; };
            Func<int> candidate = () => { candidateRan = true; return 42; };

            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(control);
                experiment.Try(candidate);
            });

            Assert.Equal(42, result);
            Assert.True(candidateRan);
            Assert.True(controlRan);
            Assert.True(((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.ExperimentName == "success").Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailure()
        {
            bool candidateRan = false;
            bool controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            Func<Task<int>> control = () => { controlRan = true; return Task.FromResult(42); };
            Func<Task<int>> candidate = () => { candidateRan = true; return Task.FromResult(43); };

            var result = await Scientist.ScienceAsync<int>("failure", experiment =>
            {
                experiment.Use(control);
                experiment.Try(candidate);
            });

            Assert.Equal(42, result);
            Assert.True(candidateRan);
            Assert.True(controlRan);
            Assert.False(TestHelper.Observation.First(m => m.ExperimentName == "failure").Matched);
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
            Assert.True(TestHelper.Observation.First(m => m.ExperimentName == "failure").Matched);
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
            bool candidateRan = false;
            bool controlRan = false;

            // We introduce side effects for testing. Don't do this in real life please.
            // Do we do a deep comparison?
            Func<int> control = () => { controlRan = true; return 42; };
            Func<int> candidate = () => { candidateRan = true; return 42; };

            var result = Scientist.Science<int>("success", experiment =>
            {
                experiment.Use(control);
                experiment.Try(candidate);
            });

            Assert.Equal(42, result);
            Assert.True(candidateRan);
            Assert.True(controlRan);
            IResult observedResult = ((InMemoryObservationPublisher)Scientist.ObservationPublisher).Observations.First(m => m.ExperimentName == "success");
            Assert.True(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }
    }
}
