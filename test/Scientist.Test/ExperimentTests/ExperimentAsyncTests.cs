using FluentAssertions;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using UnitTests;
using Xunit;


public class ExperimentAsyncTests
{
    [Fact]
    public async Task ExperimentAsync_EnsureControlRunsFirst_ShouldRunControlFirst()
    {
        var mock = Substitute.For<IControlCandidateTask<int>>();
        mock.Control().Returns(Task.FromResult(42));
        mock.Candidate().Returns(Task.FromResult(42));
        const string experimentName = nameof(ExperimentAsync_EnsureControlRunsFirst_ShouldRunControlFirst);

        var resultPublisher = new InMemoryResultPublisher();
        var scientist = new Scientist(resultPublisher);

        var result = await scientist.ExperimentAsync<int>(experimentName, experiment =>
        {
            experiment.ThrowOnMismatches = true;
            experiment.EnsureControlRunsFirst();
            experiment.Use(mock.Control);
            experiment.Try("candidate", mock.Candidate);
        });

        result.Should().Be(42);

        Received.InOrder(() =>
        {
            mock.Received().Control();
            mock.Received().Candidate();
        });

        resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();
    }
}

