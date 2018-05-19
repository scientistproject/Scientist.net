﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub;
using NSubstitute;
using UnitTests;
using Xunit;

public class TheProfessorClass
{
    public class TheScienceMethod
    {
        [Fact]
        public void ConstructorThrowsIfResultsPublisherIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Professor(null));
        }

        [Fact]
        public void DoesntRunCandidate()
        {
            const int expectedResult = 42;

            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.RunIf().Returns(false);

            const string experimentName = "successRunIf";

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.RunIf(mock.RunIf);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(expectedResult, result);

            mock.DidNotReceive().Candidate();
            mock.Received().Control();
            Assert.False(professor.ResultPublisher.Results<int>(experimentName).Any());
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndMatchesExceptions()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => { throw new InvalidOperationException(); });
            mock.Candidate().Returns(x => { throw new InvalidOperationException(); });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndMatchesExceptions);

            var ex = Assert.Throws<AggregateException>(() =>
            {
                professor.Science<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<InvalidOperationException>(baseException);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(professor.ResultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerException()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<Task<int>>>();
            var controlException = new InvalidOperationException(null, new Exception());
            var candidateException = new InvalidOperationException(null, new Exception());
            mock.Control().Returns<Task<int>>(x => { throw controlException; });
            mock.Candidate().Returns<Task<int>>(x => { throw controlException; });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndThrowsCorrectInnerException);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await professor.ScienceAsync<int>(experimentName, experiment =>
                {
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccess()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccess);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(professor.ResultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public async Task RunsBothBranchesOfTheExperimentAsyncAndReportsFailure()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(Task.FromResult(43));
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAsyncAndReportsFailure);

            var result = await professor.ScienceAsync<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.False(professor.ResultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public void AllowsReturningNullFromControlOrTest()
        {
            var professor = new Professor();
            const string experimentName = nameof(AllowsReturningNullFromControlOrTest);
            var result = professor.Science<object>(experimentName, experiment =>
            {
                experiment.Use(() => null);
                experiment.Try("candidate", () => null);
            });

            Assert.Null(result);
            Assert.True(professor.ResultPublisher.Results<object>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentAndReportsSuccessWithDurations);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();

            var observedResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.True(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void AnExceptionReportsDuration()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Throws(new InvalidOperationException());
            const string experimentName = nameof(AnExceptionReportsDuration);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(42, result);
            mock.Received().Control();
            mock.Received().Candidate();
            var observedResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.False(observedResult.Matched);
            Assert.True(observedResult.Control.Duration.Ticks > 0);
            Assert.True(observedResult.Observations.All(o => o.Duration.Ticks > 0));
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsSuccess);

            var result = professor.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(professor.ResultPublisher.Results<ComplexResult>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester2" });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithResultComparisonSetAndReportsFailure);

            var result = professor.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Compare((a, b) => a.Count == b.Count && a.Name == b.Name);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.False(professor.ResultPublisher.Results<ComplexResult>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsSuccess);

            var result = professor.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.True(professor.ResultPublisher.Results<ComplexResult>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<ComplexResult>>();
            mock.Control().Returns(new ComplexResult { Count = 10, Name = "Tester" });
            mock.Candidate().Returns(new ComplexResult { Count = 10, Name = "Tester2" });
            const string experimentName = nameof(RunsBothBranchesOfTheExperimentWithIEqualitySetAndReportsFailure);

            var result = professor.Science<ComplexResult>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            Assert.Equal(10, result.Count);
            Assert.Equal("Tester", result.Name);
            mock.Received().Control();
            mock.Received().Candidate();
            Assert.False(professor.ResultPublisher.Results<ComplexResult>(experimentName).First().Matched);
        }

        [Fact]
        public void RunsBeforeRun()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(RunsBeforeRun);

            var result = professor.Science<int>(experimentName, experiment =>
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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            var mockTwo = Substitute.For<IControlCandidate<int>>();
            mockTwo.Candidate().Returns(42);
            var mockThree = Substitute.For<IControlCandidate<int>>();
            mockThree.Candidate().Returns(42);

            const string experimentName = nameof(AllTrysAreRun);

            var result = professor.Science<int>(experimentName, experiment =>
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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            var mockTwo = Substitute.For<IControlCandidate<int>>();
            mockTwo.Candidate().Returns(42);
            var mockThree = Substitute.For<IControlCandidate<int>>();
            mockThree.Candidate().Returns(0);

            const string experimentName = nameof(SingleCandidateDifferenceCausesMismatch);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate one", mock.Candidate);
                experiment.Try("candidate two", mockTwo.Candidate);
                experiment.Try("candidate three", mockThree.Candidate);
            });

            Assert.Equal(42, result);
            Assert.False(professor.ResultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public void CallingDefaultTryTwiceThrows()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingDefaultTryTwiceThrows);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = professor.Science<int>(experimentName, experiment =>
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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingTryWithSameCandidateNameThrows);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = professor.Science<int>(experimentName, experiment =>
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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(CallingTryWithDifferentCandidateNamesIsAllowed);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
                experiment.Try("candidate2", mock.Candidate);
            });

            Assert.Equal(42, result);
            Assert.True(professor.ResultPublisher.Results<int>(experimentName).First().Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfIgnoreTrue()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfIgnoreTrue);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentRunsIfIgnoreFalse()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentRunsIfIgnoreFalse);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.True(experimentResult.MismatchedObservations.Any());
            Assert.False(experimentResult.IgnoredObservations.Any());
            Assert.False(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentRunsIfMultipleIgnoresAllFalse()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentRunsIfMultipleIgnoresAllFalse);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.True(experimentResult.MismatchedObservations.Any());
            Assert.False(experimentResult.IgnoredObservations.Any());
            Assert.False(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfMultipleIgnoresAllTrue()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfMultipleIgnoresAllTrue);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => true);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredIfMultipleIgnoresOnlyOneTrue()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredIfMultipleIgnoresOnlyOneTrue);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => true);
                experiment.Ignore((control, candidate) => false);
                experiment.Ignore((control, candidate) => false);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void ExperimentIgnoredWithComplexIgnoreFunc()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(-1);
            const string experimentName = nameof(ExperimentIgnoredWithComplexIgnoreFunc);

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
                experiment.Ignore((control, candidate) => control > 0 && candidate == -1);
            });

            Assert.Equal(42, result);
            var experimentResult = professor.ResultPublisher.Results<int>(experimentName).First();
            Assert.False(experimentResult.MismatchedObservations.Any());
            Assert.True(experimentResult.IgnoredObservations.Any());
            Assert.True(experimentResult.Matched);
        }

        [Fact]
        public void SingleContextIncludedWithPublish()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(SingleContextIncludedWithPublish);

            var result = professor.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", "data");
            });

            var publishResults = professor.ResultPublisher.Results<int>(experimentName).First();

            Assert.Equal(42, result);
            Assert.Equal(1, publishResults.Contexts.Count);

            var context = publishResults.Contexts.First();
            Assert.Equal("test", context.Key);
            Assert.Equal("data", context.Value);
        }

        [Fact]
        public void MultipleContextsIncludedWithPublish()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(MultipleContextsIncludedWithPublish);

            var result = professor.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", "data");
                e.AddContext("test2", "data2");
                e.AddContext("test3", "data3");
            });

            var publishResults = professor.ResultPublisher.Results<int>(experimentName).First();

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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(ContextReturnsComplexObjectInPublish);

            var testTime = DateTime.UtcNow;

            var result = professor.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
                e.AddContext("test", new {Id = 1, Name = "name", Date = testTime});
            });

            var publishResults = professor.ResultPublisher.Results<int>(experimentName).First();

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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(ContextEmptyIfNoContextSupplied);

            var result = professor.Science<int>(experimentName, e =>
            {
                e.Use(mock.Control);
                e.Try(mock.Candidate);
            });

            var publishResults = professor.ResultPublisher.Results<int>(experimentName).First();

            Assert.Equal(42, result);
            Assert.False(publishResults.Contexts.Any());
        }
        
        [Fact]
        public void AddContextThrowsIfDuplicateKeyAdded()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);

            const string experimentName = nameof(AddContextThrowsIfDuplicateKeyAdded);

            Assert.Throws<ArgumentException>(() =>
            {
                professor.Science<int>(experimentName, e =>
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
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(x => 1);
            mock.Candidate().Returns(x => 2);
            const string experimentName = nameof(ThrowsMismatchException);

            var ex = Assert.Throws<AggregateException>(() =>
            {
                professor.Science<int>(experimentName, experiment =>
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

            var result = professor.ResultPublisher.Results<int>(experimentName).First();
            var mismatchException = (MismatchException<int, int>)baseException;
            Assert.Equal(experimentName, mismatchException.Name);
            Assert.Same(result, mismatchException.Result);
        }

        [Fact]
        public void ScientistDisablesExperiment()
        {
            const int expectedResult = 42;

            var professor = new ProfessorThatIsNotEnabled();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);

            var result = professor.Science<int>(nameof(ScientistDisablesExperiment), experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(expectedResult, result);
            mock.DidNotReceive().Candidate();
            mock.Received().Control();
        }

        [Fact]
        public void KeepingItClean()
        {
            const int expectedResult = 42;
            const string expectedCleanResult = "Forty Two";

            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int, string>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);
            mock.Clean(expectedResult).Returns(expectedCleanResult);

            var result = professor.Science<int, string>(nameof(KeepingItClean), experiment =>
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
                professor.ResultPublisher.Results<int, string>(nameof(KeepingItClean)).First().Control.CleanedValue);

            mock.Received().Clean(expectedResult);
        }

        [Fact]
        public void ThrowsArgumentExceptionWhenConcurrentTasksInvalid()
        {
            var resultsPublisher = Substitute.For<IResultPublisher>();
            var professor = new Professor(resultsPublisher);

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                professor.ConcurrentTasks = 0;
            });

            Exception baseException = ex.GetBaseException();
            Assert.IsType<ArgumentException>(baseException);
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

            var professor = new Professor()
            {
                ConcurrentTasks = concurrentTasks
            };

            // Run the experiment
            string experimentName = nameof(RunsTasksConcurrently) + concurrentTasks;
            await professor.ScienceAsync<KeyValuePair<int, int>>(experimentName, experiment =>
            {
                // Add our control and experiments
                experiment.Use(task);
                for (int idx = 2; idx <= totalTasks; idx++)
                {
                    experiment.Try($"experiment{idx}", task);
                }
            });

            // Get the test result
            var result = professor.ResultPublisher.Results<KeyValuePair<int, int>>(experimentName).First();

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

            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);

            const string experimentName = "noCandidate";

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(mock.Control);
            });

            Assert.Equal(expectedResult, result);
            mock.Received().Control();
        }

        [Fact]
        public void NoCandidateMeansCleanIsNeverRun()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int, int>>();
            mock.Clean(Arg.Any<int>()).Returns(0);

            professor.Science<int, int>("noCandidateNoClean", experiment =>
            {
                experiment.Use(() => 42);
                experiment.Clean(mock.Clean);
            });

            mock.DidNotReceive().Clean(Arg.Any<int>());
        }

        [Fact]
        public void NoCandidateMeansRunIfIsNeverRun()
        {
            var professor = new Professor();
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.RunIf().Returns(false);

            professor.Science<int, int>("noCandidateNoRunIf", experiment =>
            {
                experiment.Use(() => 42);
                experiment.RunIf(mock.RunIf);
            });

            mock.DidNotReceive().RunIf();
        }

        [Fact]
        public void NoCandidateMeansNoDisable()
        {
            var professor = new Professor();
            var settings = Substitute.For<IScientistSettings>();
            settings.Enabled().Returns(Task.FromResult(false));

            using (Swap.Enabled(settings.Enabled))
            {
                professor.Science<int>("noCandidateNoDisable", experiment =>
                {
                    experiment.Use(() => 42);
                });

                settings.DidNotReceive().Enabled();
            }
        }

        [Fact]
        public void NoCandidateMeansNoPublish()
        {
            var professor = new Professor();
            const string experimentName = "noCandidateNoPublish";

            var result = professor.Science<int>(experimentName, experiment =>
            {
                experiment.Use(() => 42);
            });

            // verify that no results were published for this experiment
            Assert.Empty(professor.ResultPublisher.Results<int>(experimentName));
        }

        private sealed class ProfessorThatIsNotEnabled : Professor
        {
            protected override Task<bool> EnabledAsync() => Task.FromResult(false);
        }
    }
}