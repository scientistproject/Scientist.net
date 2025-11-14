using GitHub;
using GitHub.Internals;
using NSubstitute;
using System.Linq;
using UnitTests;
using Xunit;
using FluentAssertions;

namespace Github.ExperimentTests
{
    public class ExperimentTests
    {
        [Fact]
        public void Experiment_EnsureControlRunsFirst_ShouldRunControlFirst()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(Experiment_EnsureControlRunsFirst_ShouldRunControlFirst);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.ThrowOnMismatches = true;
                experiment.EnsureControlRunsFirst();
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            result.Should().Be(42);
            Received.InOrder(() => {
                mock.Received().Control();
                mock.Received().Candidate();
            });
            resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();
        }
    }
}
