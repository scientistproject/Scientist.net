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
        public void DoesntRunCandidate()
        {
            const int expectedResult = 42;

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.RunIf().Returns(false);

            const string experimentName = "successRunIf";

            var experiment = Scientist.Science<int>(experimentName);

            var result = experiment.Where(() => mock.RunIf())
                .Use(() => mock.Control())
                .Try(() => mock.Candidate())
                .Execute();

            Assert.Equal(expectedResult, result);

            mock.DidNotReceive().Candidate();
            mock.Received().Control();
            Assert.False(TestHelper.Results<int>().Any(m => m.ExperimentName == experimentName));
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndMatchesExceptions()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => { throw new InvalidOperationException(); });
            mock.Candidate().Returns(x => { throw new InvalidOperationException(); });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndMatchesExceptions);

            var experiment = Scientist.Science<int>(experimentName);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                experiment.Use(() => mock.Control())
                    .Try(() => mock.Candidate())
                    .Execute();
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccess);

            var experiment = Scientist.Science<int>(experimentName);

            var result = experiment.Use(() => mock.Control()).Try(() => mock.Candidate()).Execute();
            
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAsyncAndReportsFailure);

            var experiment = Scientist.Science<int>(experimentName);

            var result = await experiment.Use(mock.Control).Try(mock.Candidate).ExecuteAsync();
            
            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void AllowsReturningNullFromControlOrTest()
        {
            const string experimentName = nameof(AllowsReturningNullFromControlOrTest);

            var experiment = Scientist.Science<object>(experimentName);

            //Requires (object)null casting, otherwise calls Func<Task<T>> overload and fails
            var result = experiment.Use(() => (object)null).Try(() => (object)null).Execute();
            
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations);

            var experiment = Scientist.Science<int>(experimentName);

            var result = experiment.Use(() => mock.Control()).Try(() => mock.Candidate()).Execute();
            
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
            const string experimentName = nameof(AnExceptionReportsDuration);

            var experiment = Scientist.Science<int>(experimentName);

            var result = experiment.Use(() => mock.Control()).Try(() => mock.Candidate()).Execute();

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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess);

            var experiment = Scientist.Science<ComplexResult>(experimentName);

            var result = experiment.Use(() => mock.Control())
                .Try(() => mock.Candidate())
                .WithComparer((a, b) => a.Count == b.Count && a.Name == b.Name)
                .Execute();
            
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure);

            var experiment = Scientist.Science<ComplexResult>(experimentName);

            var result = experiment.Use(() => mock.Control())
                .Try(() => mock.Candidate())
                .WithComparer((a, b) => a.Count == b.Count && a.Name == b.Name)
                .Execute();
            
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess);

            var experiment = Scientist.Science<ComplexResult>(experimentName);

            var result = experiment.Use(() => mock.Control()).Try(() => mock.Candidate()).Execute();
            
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure);

            var experiment = Scientist.Science<ComplexResult>(experimentName);

            var result = experiment.Use(() => mock.Control()).Try(() => mock.Candidate()).Execute();

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
            const string experimentName = nameof(RunsBeforeRun);

            var experiment = Scientist.Science<int>(experimentName);

            var result =
                experiment.Use(() => mock.Control())
                    .Try(() => mock.Candidate())
                    .BeforeRun(() => mock.BeforeRun())
                    .Execute();
            
            Assert.Equal(42, result);
            mock.Received().BeforeRun();
        }
    }
}
