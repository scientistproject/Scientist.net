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
            const string experimentName = "RunsBothBranchesOfTheExperimentAndMatchesExceptions";

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
            const string experimentName = "RunsBothBranchesOfTheExperimentAndReportsSuccess";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailure()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(Task.FromResult(43));
            const string experimentName = "RunsBothBranchesOfTheExperimentAsyncAndReportsFailure";

            var result = await Scientist.ScienceAsync<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void AllowsReturningNullFromControlOrTest()
        {
            const string experimentName = "AllowsReturningNullFromControlOrTest";
            var result = Scientist.Science<object>(experimentName, experiment =>
            {
                experiment.Use(() => null);
                experiment.Try(() => null);
            });

            Assert.Null(result);
            Assert.True(TestHelper.Results<object>().First(m => m.ExperimentName == experimentName).Matched);
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
            const string experimentName = "RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();

            Result<int> observedResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.True(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void AnExceptionReportsDuration()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Throws(new InvalidOperationException());
            const string experimentName = "AnExceptionReportsDuration";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Result<int> observedResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.False(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess()
        {
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            const string experimentName = "RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess";

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
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
            const string experimentName = "RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure";

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
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
            const string experimentName = "RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess";

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
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
            const string experimentName = "RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure";

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
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
            const string experimentName = "RunsBeforeRun";

            var result = Scientist.Science<int>(experimentName, experiment =>
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
