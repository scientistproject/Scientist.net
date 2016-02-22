using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
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
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == "success").Matched);
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
            Assert.False(TestHelper.Results<int>().First(m => m.ExperimentName == "failure").Matched);
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
            Assert.True(TestHelper.Results<object>().First(m => m.ExperimentName == "failure").Matched);
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
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == "success").Matched);
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == "success").Control.Duration.Ticks > 0);
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == "success").Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void AnExceptionReportsDuration()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Throws(new InvalidOperationException());

            var result = Scientist.Science<int>("failure", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Result<int> observedResult = TestHelper.Results<int>().First(m => m.ExperimentName == "success");
            Assert.True(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess()
        {
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester" });

            var result = Scientist.Science<ComplexResult>("success", experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(TestHelper.Results<ComplexResult>().First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure()
        {
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester2" });

            var result = Scientist.Science<ComplexResult>("success", experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.False(TestHelper.Results<ComplexResult>().First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess()
        {
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester" });

            var result = Scientist.Science<ComplexResult>("success", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(TestHelper.Results<ComplexResult>().First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure()
        {
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester2" });

            var result = Scientist.Science<ComplexResult>("success", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.False(TestHelper.Results<ComplexResult>().First().Matched);
        }

        [Fact]
        public void RunsBeforeRun()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            
            var result = Scientist.Science<int>("beforeRun", experiment =>
            {
                experiment.BeforeRun(mock.BeforeRun);
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().BeforeRun();
        }
    }
}
