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
    //TODO: Clean up this class
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

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.RunIf(mock.RunIf);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

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

            var ex = Assert.Throws<AggregateException>(() =>
            {
                Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
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
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccess);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAsyncAndReportsFailure);

            var result = await Scientist.ScienceAsync<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void AllowsReturningNullFromControlOrTest()
        {
            const string experimentName = nameof(AllowsReturningNullFromControlOrTest);
            var result = Scientist.Science<object>(experimentName, experiment =>
            {
                experiment.Use(() => null);
                experiment.Try("candidate", () => null);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(AnExceptionReportsDuration);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess);

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure);

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess);

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure);

            var result = Scientist.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
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
            const string experimentName = nameof(RunsBeforeRun);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.BeforeRun(mock.BeforeRun);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().BeforeRun();
        }

        [Fact]
        public void AllTrysAreRun()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            var mockTwo = Substitute.For<IControlCandidate<int>>();
            mockTwo.Candidate().Returns(42);
            var mockThree = Substitute.For<IControlCandidate<int>>();
            mockThree.Candidate().Returns(42);

            const string experimentName = nameof(AllTrysAreRun);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate one", mock.Candidate);
                experiment.Try("candidate two", mockTwo.Candidate);
                experiment.Try("candidate three", mockThree.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Candidate();
            mockTwo.Received().Candidate();
            mockThree.Received().Candidate();
        }

        [Fact]
        public void SingleCandidateDifferenceCausesMismatch()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            var mockTwo = Substitute.For<IControlCandidate<int>>();
            mockTwo.Candidate().Returns(42);
            var mockThree = Substitute.For<IControlCandidate<int>>();
            mockThree.Candidate().Returns(0);

            const string experimentName = nameof(SingleCandidateDifferenceCausesMismatch);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate one", mock.Candidate);
                experiment.Try("candidate two", mockTwo.Candidate);
                experiment.Try("candidate three", mockThree.Candidate);
            });

            Assert.Equal(42, result);
            Assert.False(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void CallingDefaultTryTwiceThrows()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingDefaultTryTwiceThrows);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                    experiment.Try(mock.Candidate);
                });
            });
        }

        [Fact]
        public void CallingTryWithSameCandidateNameThrows()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingTryWithSameCandidateNameThrows);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
        }

        [Fact]
        public void CallingTryWithDifferentCandidateNamesIsAllowed()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingTryWithDifferentCandidateNamesIsAllowed);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
                experiment.Try("candidate2", mock.Candidate);
            });

            Assert.Equal(42, result);
            Assert.True(TestHelper.Results<int>().First(m => m.ExperimentName == experimentName).Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfIgnoreTrue()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfIgnoreTrue);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentRunsIfIgnoreFalse()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentRunsIfIgnoreFalse);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.True(experimentResult.MismatchedObservations.Any());
            Assert.False(experimentResult.IgnoredObservations.Any());
            Assert.False(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentRunsIfMultipleIgnoresAllFalse()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentRunsIfMultipleIgnoresAllFalse);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.True(experimentResult.MismatchedObservations.Any());
            Assert.False(experimentResult.IgnoredObservations.Any());
            Assert.False(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfMultipleIgnoresAllTrue()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfMultipleIgnoresAllTrue);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => true);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfMultipleIgnoresOnlyOneTrue()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfMultipleIgnoresOnlyOneTrue);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredWithComplexIgnoreFunc()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredWithComplexIgnoreFunc);

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => control > 0 && candidate == -1);
            });

            Assert.Equal(42, result);
            var experimentResult = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void SingleContextIncludedWithPublish()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(SingleContextIncludedWithPublish);

            var result = Scientist.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", "data");
            });

            var publishResults = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);

            Assert.Equal(42, result);
            Assert.Equal(1, publishResults.Contexts.Count);

            var context = publishResults.Contexts.First();
            Assert.Equal("test", context.Key);
            Assert.Equal("data", context.Value);
        }

        [Fact]
        public void MultipleContextsIncludedWithPublish()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(MultipleContextsIncludedWithPublish);

            var result = Scientist.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", "data");
                e.AddContext("test2", "data2");
                e.AddContext("test3", "data3");
            });

            var publishResults = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);

            Assert.Equal(42, result);
            Assert.Equal(3, publishResults.Contexts.Count);

            var context = publishResults.Contexts.First();
            Assert.Equal("test", context.Key);
            Assert.Equal("data", context.Value);

            context = publishResults.Contexts.Skip(1).First();
            Assert.Equal("test2", context.Key);
            Assert.Equal("data2", context.Value);

            context = publishResults.Contexts.Skip(2).First();
            Assert.Equal("test3", context.Key);
            Assert.Equal("data3", context.Value);
        }

        [Fact]
        public void ContextReturnsComplexObjectInPublish()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(ContextReturnsComplexObjectInPublish);

            var testTime = DateTime.UtcNow;

            var result = Scientist.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", new {Id = 1, Name = "name", Date = testTime});
            });

            var publishResults = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);

            Assert.Equal(42, result);
            Assert.Equal(1, publishResults.Contexts.Count);

            var context = publishResults.Contexts.First();
            Assert.Equal("test", context.Key);
            Assert.Equal(1, context.Value.Id);
            Assert.Equal("name", context.Value.Name);
            Assert.Equal(testTime, context.Value.Date);
        }

        [Fact]
        public void ContextEmptyIfNoContextSupplied()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(ContextEmptyIfNoContextSupplied);

            var result = Scientist.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
            });

            var publishResults = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);

            Assert.Equal(42, result);
            Assert.False(publishResults.Contexts.Any());
        }
        
        [Fact]
        public void AddContextThrowsIfDuplicateKeyAdded()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(AddContextThrowsIfDuplicateKeyAdded);

            Assert.Throws<ArgumentException>(() =>
            {
                Scientist.Science<int>(experimentName, e =>
                {
                    e.Use(mock.Control);
                    e.Try(mock.Candidate);
                    e.AddContext("test", "data");
                    e.AddContext("test", "data");
                });
            });
        }

        [Fact]
        public void ThrowsMismatchException()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => 1);
            mock.Candidate().Returns(x => 2);
            const string experimentName = nameof(ThrowsMismatchException);

            var ex = Assert.Throws<AggregateException>(() =>
            {
                Scientist.Science<int>(experimentName, experiment =>
                {
                    experiment.ThrowOnMismatches = true;
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<MismatchException<int>>(baseException);
            mock.Received().Control();
            mock.Received().Candidate();

            var result = TestHelper.Results<int>().First(m => m.ExperimentName == experimentName);
            var mismatchException = (MismatchException<int>)baseException;
            Assert.Equal(experimentName, mismatchException.Name);
            Assert.Same(result, mismatchException.Result);
        }
    }
}
