using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub;
using NSubstitute;
using UnitTests;
using Xunit;

[Collection("Tests Dependent Upon static ResultPublisher that can't run in parallel")]
public class FireAndForgetResultPublisherTests
{
    [Fact]
    public async Task PublishesAsynchronously()
    {
        const int expectedResult = 42;
        var pendingPublishTask = new TaskCompletionSource<object>();

        // Create a new publisher that will delay all
        // publishing to account for this test.
        var innerPublisher = Substitute.For<IResultPublisher>();
        innerPublisher.Publish(Arg.Any<Result<int, int>>())
            .Returns(call => pendingPublishTask.Task);

        var fireAndForgetPublisher = new FireAndForgetResultPublisher(innerPublisher, ex => { });

        var mock = Substitute.For<IControlCandidate<int, string>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(expectedResult);

        const int count = 10;
        using (Swap.Publisher(fireAndForgetPublisher))
        {
            Parallel.ForEach(
                Enumerable.Repeat(0, count),
                src =>
                {
                    var result = Scientist.Science<int>("myExperiment", experiment =>
                    {
                        experiment.Use(mock.Control);
                        experiment.Try(mock.Candidate);
                    });

                    Assert.Equal(expectedResult, result);
                });
        }

        // Make sure that the above science calls are still publishing.
        Task whenPublished = fireAndForgetPublisher.WhenPublished();
        Assert.NotNull(whenPublished);

        // Ensure that the mock was called before the when published task has completed.
        mock.Received(count).Control();
        mock.Received(count).Candidate();

        Assert.False(whenPublished.IsCompleted, "When Published Task completed early.");

        pendingPublishTask.SetResult(null);

        await whenPublished;

        Assert.True(whenPublished.IsCompleted, "When Published Task isn't complete.");
    }

    public class PublishException : Exception { };

    [Fact]
    public async Task HandlesExceptionsThrownImmediatelyByInnerPublisher()
    {
        const int expectedResult = 42;
        var exceptionToThrow = new PublishException();
        var exceptionsThrown = new List<Exception>();

        var innerPublisher = Substitute.For<IResultPublisher>();
        innerPublisher.Publish(Arg.Any<Result<int, int>>())
            .Throws(exceptionToThrow);

        var fireAndForgetPublisher = new FireAndForgetResultPublisher(innerPublisher, ex => { exceptionsThrown.Add(ex); });

        var mock = Substitute.For<IControlCandidate<int, string>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(expectedResult);

        using (Swap.Publisher(fireAndForgetPublisher))
        {
            var result = Scientist.Science<int>("myExperiment", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });
        }

        await fireAndForgetPublisher.WhenPublished();

        Assert.Equal(new List<Exception> { exceptionToThrow }, exceptionsThrown);
    }

    [Fact]
    public async Task HandlesDelayedExceptionsThrownByInnerPublisher()
    {
        const int expectedResult = 42;
        var exceptionToThrow = new PublishException();
        var exceptionsThrown = new List<Exception>();

        var pendingPublishTask = new TaskCompletionSource<object>();
        var innerPublisher = Substitute.For<IResultPublisher>();
        innerPublisher.Publish(Arg.Any<Result<int, int>>())
            .Returns(call => pendingPublishTask.Task.ContinueWith(_ => { throw exceptionToThrow; }));

        var fireAndForgetPublisher = new FireAndForgetResultPublisher(innerPublisher, ex => { exceptionsThrown.Add(ex); });

        var mock = Substitute.For<IControlCandidate<int, string>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(expectedResult);

        using (Swap.Publisher(fireAndForgetPublisher))
        {
            var result = Scientist.Science<int>("myExperiment", experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });
        }

        var whenPublished = fireAndForgetPublisher.WhenPublished();

        Assert.False(whenPublished.IsCompleted, "When Published Task completed early.");

        pendingPublishTask.SetResult(null);

        await whenPublished;

        Assert.True(whenPublished.IsCompleted, "When Published Task isn't complete.");

        Assert.Equal(new List<Exception> { exceptionToThrow }, exceptionsThrown);
    }
}
