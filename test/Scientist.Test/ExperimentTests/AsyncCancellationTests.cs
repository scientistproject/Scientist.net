using FluentAssertions;
using Github.Ordering;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

public class AsyncCancellationTests
{
    public class WithCancellationTokenTests
    {
        [Fact]
        public async Task When_cancelled_during_control_run_should_throw_operation_cancelled()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(async call =>
            {
                await Task.Delay(5000);
                return 42;
            });
            mock.Candidate().Returns(Task.FromResult(37));

            const string experimentName = nameof(When_cancelled_during_control_run_should_throw_operation_cancelled);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var cts = new CancellationTokenSource();

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await scientist.ExperimentAsync<int>(experimentName, experiment =>
                {
                    experiment.WithCancellationToken(cts.Token);
                    experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
        }

        [Fact]
        public async Task When_cancelled_after_control_has_ran_should_return_cancelled_in_results()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(async call =>
            {
                await Task.Delay(2000);
                return 37;
            });

            const string experimentName = nameof(When_cancelled_after_control_has_ran_should_return_cancelled_in_results);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var cts = new CancellationTokenSource();

            cts.CancelAfter(200);

            var result = await scientist.ExperimentAsync<int>(experimentName, 1, experiment =>
            {
                experiment.WithCancellationToken(cts.Token);
                experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            result.Should().Be(42);
            await mock.Received().Control();
            await mock.Received().Candidate();
            Assert.True(resultPublisher.Results<int>(experimentName).First().Cancelled);
        }
    }

    public class UseTests
    {
        [Fact]
        public async Task When_cancelled_should_throw_operation_cancelled()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(async call =>
            {
                await Task.Delay(5000);
                return 42;
            });
            mock.Candidate().Returns(Task.FromResult(37));

            const string experimentName = nameof(When_cancelled_should_throw_operation_cancelled);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var cts = new CancellationTokenSource();

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var result = await scientist.ExperimentAsync<int>(experimentName, experiment =>
                {
                    experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
                    experiment.Use(mock.Control, cts.Token);
                    experiment.Try("candidate", mock.Candidate);
                });
            });
        }
    }

    public class TryTests
    {
        [Fact]
        public async Task When_cancelled_should_return_cancelled_in_results()
        {
            var mock = Substitute.For<IControlCandidateTask<int>>();
            mock.Control().Returns(Task.FromResult(42));
            mock.Candidate().Returns(async call =>
            {
                await Task.Delay(5000);
                return 37;
            });

            const string experimentName = nameof(When_cancelled_should_return_cancelled_in_results);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var cts = new CancellationTokenSource();

            cts.Cancel();

            var result = await scientist.ExperimentAsync<int>(experimentName, experiment =>
            {
                experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate, cts.Token);
            });

            result.Should().Be(42);

            await mock.Received().Control();
            await mock.DidNotReceive().Candidate();
            resultPublisher.Results<int>(experimentName).First().Candidates[0].Cancelled.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Overriden_control_cancellation_token_should_throw_operation_cancelled()
    {
        var mock = Substitute.For<IControlCandidateTask<int>>();
        mock.Control().Returns(async call =>
        {
            await Task.Delay(5000);
            return 42;
        });
        mock.Candidate().Returns(Task.FromResult(37));

        const string experimentName = nameof(Overriden_control_cancellation_token_should_throw_operation_cancelled);

        var resultPublisher = new InMemoryResultPublisher();
        var scientist = new Scientist(resultPublisher);

        var globalCts = new CancellationTokenSource();
        var controlCts = new CancellationTokenSource();

        controlCts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await scientist.ExperimentAsync<int>(experimentName, experiment =>
            {
                experiment.WithCancellationToken(globalCts.Token);
                experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
                experiment.Use(mock.Control, controlCts.Token);
                experiment.Try("candidate", mock.Candidate);
            });
        });
    }

    [Fact]
    public async Task Overriden_candidate_cancellation_token_should_show_cancelled_in_results()
    {
        var mock = Substitute.For<IControlCandidateTask<int>>();
        mock.Control().Returns(Task.FromResult(42));
        mock.Candidate().Returns(async call =>
        {
            await Task.Delay(2000);
            return 37;
        });

        const string experimentName = nameof(Overriden_candidate_cancellation_token_should_show_cancelled_in_results);

        var resultPublisher = new InMemoryResultPublisher();
        var scientist = new Scientist(resultPublisher);

        var globalCts = new CancellationTokenSource();
        var candidateCts = new CancellationTokenSource();

        candidateCts.Cancel();

        var result = await scientist.ExperimentAsync<int>(experimentName, experiment =>
         {
             experiment.WithCancellationToken(globalCts.Token);
             experiment.UseCustomOrdering(behaviours => Task.FromResult(Ordering.ControlFirst(behaviours)));
             experiment.Use(mock.Control);
             experiment.Try("candidate", mock.Candidate, candidateCts.Token);
         });

        result.Should().Be(42);
        await mock.Received().Control();
        await mock.DidNotReceive().Candidate();
        Assert.True(resultPublisher.Results<int>(experimentName).First().Cancelled);
    }
}