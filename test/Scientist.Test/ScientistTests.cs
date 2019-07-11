using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using UnitTests;
using Xunit;

public class TheScientistClass
{
    //TODO: Clean up this class
    [Collection("Tests Dependent Upon static ResultPublisher that can't run in parallel")]
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
            Assert.False(TestHelper.Results<int>(experimentName).Any());
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
            Assert.True(TestHelper.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerException()
        {
            var mock = Substitute.For<IControlCandidate<Task<int>>>();
            var controlException = new InvalidOperationException(null, new Exception());
            var candidateException = new InvalidOperationException(null, new Exception());
            mock.Control().Returns<Task<int>>(x => { throw controlException; });
            mock.Candidate().Returns<Task<int>>(x => { throw controlException; });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerException);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Scientist.ScienceAsync<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
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
            Assert.True(TestHelper.Results<int>(experimentName).First().Matched);
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
            Assert.False(TestHelper.Results<int>(experimentName).First().Matched);
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
            Assert.True(TestHelper.Results<object>(experimentName).First().Matched);
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

            var observedResult = TestHelper.Results<int>(experimentName).First();
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
            var observedResult = TestHelper.Results<int>(experimentName).First();
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
            Assert.True(TestHelper.Results<ComplexResult>(experimentName).First().Matched);
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
            Assert.False(TestHelper.Results<ComplexResult>(experimentName).First().Matched);
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
            Assert.True(TestHelper.Results<ComplexResult>(experimentName).First().Matched);
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
            Assert.False(TestHelper.Results<ComplexResult>(experimentName).First().Matched);
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
            Assert.False(TestHelper.Results<int>(experimentName).First().Matched);
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
            Assert.True(TestHelper.Results<int>(experimentName).First().Matched);
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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
            var experimentResult = TestHelper.Results<int>(experimentName).First();
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

            var publishResults = TestHelper.Results<int>(experimentName).First();

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

            var publishResults = TestHelper.Results<int>(experimentName).First();

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

            var publishResults = TestHelper.Results<int>(experimentName).First();

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

            var publishResults = TestHelper.Results<int>(experimentName).First();

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
            Assert.IsType<MismatchException<int, int>>(baseException);
            mock.Received().Control();
            mock.Received().Candidate();

            var result = TestHelper.Results<int>(experimentName).First();
            var mismatchException = (MismatchException<int, int>)baseException;
            Assert.Equal(experimentName, mismatchException.Name);
            Assert.Same(result, mismatchException.Result);
        }

        [Fact]
        public void ScientistDisablesExperiment()
        {
            const int expectedResult = 42;

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);

            var settings = Substitute.For<IScientistSettings>();
            settings.Enabled().Returns(Task.FromResult(false));
            using (Swap.Enabled(settings.Enabled))
            {
                var result = Scientist.Science<int>(nameof(ScientistDisablesExperiment), experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                });

                Assert.Equal(expectedResult, result);
                mock.DidNotReceive().Candidate();
                mock.Received().Control();
                settings.Received().Enabled();
            }
        }

        [Fact]
        public void ScientistDisablesAllExperiments()
        {
            const int expectedResult = 42;

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);

            var settings = Substitute.For<IScientistSettings>();
            settings.Enabled().Returns(Task.FromResult(false));

            var instanceMock = Substitute.For<IControlCandidate<int>>();
            instanceMock.Control().Returns(expectedResult);
            instanceMock.Candidate().Returns(0);

            using (Swap.Enabled(settings.Enabled))
            {

                var result = Scientist.Science<int>(nameof(ScientistDisablesExperiment), experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                });

                mock.DidNotReceive().Candidate();
                mock.Received().Control();
                settings.Received().Enabled();

                var scientist = new Scientist(Substitute.For<IResultPublisher>());
                scientist.Experiment<int>(nameof(ScientistDisablesAllExperiments), experiment =>
                {
                    experiment.Use(instanceMock.Control);
                    experiment.Try(instanceMock.Candidate);
                });

                instanceMock.DidNotReceive().Candidate();
                instanceMock.Received().Control();
                settings.Received().Enabled();
            }
        }

        [Fact]
        public void KeepingItClean()
        {
            const int expectedResult = 42;
            const string expectedCleanResult = "Forty Two";

            var mock = Substitute.For<IControlCandidate<int, string>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);
            mock.Clean(expectedResult).Returns(expectedCleanResult);

            var result = Scientist.Science<int, string>(nameof(KeepingItClean), experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Clean(mock.Clean);
            });

            Assert.Equal(expectedResult, result);

            // Make sure that the observations aren't cleaned unless called explicitly.
            mock.DidNotReceive().Clean(expectedResult);

            Assert.Equal(
                expectedCleanResult,
                TestHelper.Results<int, string>(nameof(KeepingItClean)).First().Control.CleanedValue);

            mock.Received().Clean(expectedResult);
        }

        [Fact]
        public async Task KeepingItCleanWithParallelTasks()
        {
            const int expectedResult = 42;
            const string expectedCleanResult = "Forty Two";

            var mock = Substitute.For<IControlCandidateTask<int, string>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);
            mock.Clean(expectedResult).Returns(expectedCleanResult);

            var result = await Scientist.ScienceAsync<int, string>(nameof(KeepingItCleanWithParallelTasks), experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Clean(mock.Clean);
            });

            Assert.Equal(expectedResult, result);

            // Make sure that the observations aren't cleaned unless called explicitly.
            mock.DidNotReceive().Clean(expectedResult);

            Assert.Equal(
                expectedCleanResult,
                TestHelper.Results<int, string>(nameof(KeepingItCleanWithParallelTasks)).First().Control.CleanedValue);

            mock.Received().Clean(expectedResult);
        }

        [Fact]
        public void ThrowsArgumentExceptionWhenConcurrentTasksInvalid()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(x => 1);
            mock.Candidate().Returns(x => 2);
            const string experimentName = nameof(ThrowsArgumentExceptionWhenConcurrentTasksInvalid);

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Scientist.ScienceAsync<int>(experimentName, 0, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try(mock.Candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<ArgumentException>(baseException);
            mock.DidNotReceive().Control();
            mock.DidNotReceive().Candidate();
        }

        [Theory,
            InlineData(1),
            InlineData(2),
            InlineData(4)]
        public async Task RunsTasksConcurrently(int concurrentTasks)
        {
            // Control + 3 experiments
            var totalTasks = 1 + 3;

            // Expected number of batches
            var expectedBatches = Math.Ceiling(1D * totalTasks / concurrentTasks);

            // Use CountdownEvents to ensure tasks don't finish before all tasks in that batch have started
            var startedSignal = new CountdownEvent(concurrentTasks);
            var finishedSignal = new CountdownEvent(concurrentTasks);

            // Batch counter
            int batch = 1;

            // Our test task
            var task = new Func<Task<KeyValuePair<int, int>>>(() => 
            {
                return Task.Run(() =>
                {
                    // Signal that we have started
                    var last = startedSignal.Signal();

                    var myBatch = batch;

                    // Wait till all tasks for this batch have started
                    startedSignal.Wait();

                    // Signal we have finished
                    finishedSignal.Signal();

                    // Last task to start needs to reset the events
                    if (last)
                    {
                        // Wait for all tasks in the batch to have finished
                        finishedSignal.Wait();

                        // Reset the countdown events
                        startedSignal.Reset();
                        finishedSignal.Reset();
                        batch++;
                    }

                    // Return threadId
                    return new KeyValuePair<int, int>(myBatch, Thread.CurrentThread.ManagedThreadId);
                });
            });

            // Run the experiment
            string experimentName = nameof(RunsTasksConcurrently) + concurrentTasks;
            await Scientist.ScienceAsync<KeyValuePair<int, int>>(experimentName, concurrentTasks, experiment =>
            {
                // Add our control and experiments
                experiment.Use(task);
                for (int idx = 2; idx <= totalTasks; idx++)
                {
                    experiment.Try($"experiment{idx}", task);
                }
            });

            // Get the test result
            var result = TestHelper.Results<KeyValuePair<int, int>>(experimentName).First();

            // Consolidate the returned values from the tasks
            var results = result.Observations.Select(x => x.Value);

            // Assert correct number of batches
            Assert.Equal(expectedBatches, results.Select(x => x.Key).Distinct().Count());

            // Now check each batch
            for (int batchNo = 1; batchNo <= expectedBatches; batchNo++)
            {
                // Get the threadIds used by each task in the batch
                var batchThreadIds = results.Where(x => x.Key == batchNo).Select(x => x.Value);

                // Assert expected number of concurrent tasks in batch
                Assert.Equal(concurrentTasks, batchThreadIds.Count());

                // Assert unique threadIds in batch
                Assert.Equal(batchThreadIds.Count(), batchThreadIds.Distinct().Count());
            }
        }

        [Fact]
        public void NoCandidateStillRunsControl()
        {
            const int expectedResult = 42;

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);

            const string experimentName = "noCandidate";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
            });

            Assert.Equal(expectedResult, result);
            mock.Received().Control();
        }

        [Fact]
        public void NoCandidateMeansCleanIsNeverRun()
        {
            var mock = Substitute.For<IControlCandidate<int, int>>();
            mock.Clean(Arg.Any<int>()).Returns(0);

            Scientist.Science<int, int>("noCandidateNoClean", experiment =>
            {
                experiment.Use(() => 42);
                experiment.Clean(mock.Clean);
            });

            mock.DidNotReceive().Clean(Arg.Any<int>());
        }

        [Fact]
        public void NoCandidateMeansRunIfIsNeverRun()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.RunIf().Returns(false);

            Scientist.Science<int, int>("noCandidateNoRunIf", experiment =>
            {
                experiment.Use(() => 42);
                experiment.RunIf(mock.RunIf);
            });

            mock.DidNotReceive().RunIf();
        }

        [Fact]
        public void NoCandidateMeansNoDisable()
        {
            var settings = Substitute.For<IScientistSettings>();
            settings.Enabled().Returns(Task.FromResult(false));

            using (Swap.Enabled(settings.Enabled))
            {
                Scientist.Science<int>("noCandidateNoDisable", experiment =>
                {
                    experiment.Use(() => 42);
                });

                settings.DidNotReceive().Enabled();
            }
        }

        [Fact]
        public void NoCandidateMeansNoPublish()
        {
            const string experimentName = "noCandidateNoPublish";

            var result = Scientist.Science<int>(experimentName, experiment =>
            {
                experiment.Use(() => 42);
            });

            // verify that no results were published for this experiment
            Assert.Empty(TestHelper.Results<int>(experimentName));
        }

        [Fact]
        public void ConstructorThrowsIfResultPublisherIsNull()
        {
            IResultPublisher resultPublisher = null;

            Assert.Throws<ArgumentNullException>("resultPublisher", () => new Scientist(resultPublisher));
        }

        [Fact]
        public void DoesntRunCandidateIfIsEnabledAsyncReturnsFalse()
        {
            const int expectedResult = 42;

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new MyScientist(resultPublisher);

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);

            const string experimentName = "doesNotRunIfNotEnabled";

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(expectedResult, result);

            mock.DidNotReceive().Candidate();
            mock.Received().Control();
            Assert.False(resultPublisher.Results<int>(experimentName).Any());
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndMatchesExceptionsForInstance()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => { throw new InvalidOperationException(); });
            mock.Candidate().Returns(x => { throw new InvalidOperationException(); });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndMatchesExceptions);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var ex = Assert.Throws<AggregateException>(() =>
            {
                scientist.Experiment<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<InvalidOperationException>(baseException);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(resultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerExceptionForInstance()
        {
            var mock = Substitute.For<IControlCandidate<Task<int>>>();
            var controlException = new InvalidOperationException(null, new Exception());
            var candidateException = new InvalidOperationException(null, new Exception());
            mock.Control().Returns<Task<int>>(x => { throw controlException; });
            mock.Candidate().Returns<Task<int>>(x => { throw controlException; });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerException);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await scientist.ExperimentAsync<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccessForInstance()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccess);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(resultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailureForInstance()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(Task.FromResult(43));
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAsyncAndReportsFailure);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = await scientist.ExperimentAsync<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(resultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfSimpleSynchronousExperimentAndReportsFailure()
        {
            const string experimentName = nameof(RunsBothBranchesOfSimpleSynchronousExperimentAndReportsFailure);

            var resultPublisher = new InMemoryResultPublisher();
            var science = new Scientist(resultPublisher);

            var result = science.Experiment<int>(experimentName, experiment =>
            {
                experiment.Use(() => 42);
                experiment.Try(() => 37);
            });

            Assert.Equal(42, result);
            Assert.False(resultPublisher.Results<int>(experimentName).First().Matched);
            Assert.True(resultPublisher.Results<int>(experimentName).First().Mismatched);
        }

        [Fact]
        public async Task RunsBothBranchesOfSimpleAsynchronousExperimentAndReportsFailure()
        {
            const string experimentName = nameof(RunsBothBranchesOfSimpleAsynchronousExperimentAndReportsFailure);

            var resultPublisher = new InMemoryResultPublisher();
            var science = new Scientist(resultPublisher);

            var result = await science.ExperimentAsync<int>(experimentName, experiment =>
            {
                experiment.Use(() => Task.FromResult(42));
                experiment.Try(() => Task.FromResult(37));
            });

            Assert.Equal(42, result);
            Assert.False(resultPublisher.Results<int>(experimentName).First().Matched);
            Assert.True(resultPublisher.Results<int>(experimentName).First().Mismatched);
        }

        private sealed class MyScientist : Scientist
        {
            internal MyScientist(IResultPublisher resultPublisher)
                : base(resultPublisher)
            {
            }

            protected override Task<bool> IsEnabledAsync() => Task.FromResult(false);
        }
    }
}
