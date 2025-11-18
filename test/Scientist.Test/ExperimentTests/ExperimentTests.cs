using FluentAssertions;
using Github.Ordering;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTests;
using Xunit;

public class ExperimentTests
{
    public class UseCustomOrdering
    {
        [Fact]
        public void No_custom_ordering_specified_should_run_using_default_random()
        {
            const string experimentName = nameof(No_custom_ordering_specified_should_run_using_default_random);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var timesToRun = 10;

            var controlRanFirst = false;
            var candidateRanFirst = false;

            for (int i = 0; i < timesToRun; i++)
            {
                var mock = Substitute.For<IControlCandidate<int>>();
                mock.Control().Returns(42);
                mock.Candidate().Returns(42);

                var result = scientist.Experiment<int>(experimentName, experiment =>
                {
                    experiment.ThrowOnMismatches = true;
                    experiment.Use(mock.Control);
                    experiment.Try("candidate", mock.Candidate);
                });

                result.Should().Be(42);
                resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();

                var firstCall = mock.ReceivedCalls().First();

                if (firstCall.GetMethodInfo().Name == "Control")
                {
                    controlRanFirst = true;
                }

                if (firstCall.GetMethodInfo().Name == "Candidate")
                {
                    candidateRanFirst = true;
                }
            }

            controlRanFirst.Should().BeTrue($"Out of {timesToRun} runs, control should be seen to be ran first at least once");
            candidateRanFirst.Should().BeTrue($"Out of {timesToRun} runs, candidate should be seen to be ran first at least once");
        }

        [Fact]
        public void ControlFirst_specified_should_run_first()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(ControlFirst_specified_should_run_first);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.ThrowOnMismatches = true;
                experiment.UseCustomOrdering(Ordering.ControlFirst);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            result.Should().Be(42);
            resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();
            Received.InOrder(() =>
            {
                mock.Received().Control();
                mock.Received().Candidate();
            });
        }

        [Fact]
        public void ControlLast_specified_should_run_last()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(ControlLast_specified_should_run_last);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.ThrowOnMismatches = true;
                experiment.UseCustomOrdering(Ordering.ControlLast);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            result.Should().Be(42);
            resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();
            Received.InOrder(() =>
            {
                mock.Received().Candidate();
                mock.Received().Control();
            });
        }

        [Fact]
        public void Passed_custom_ordering_method_runs()
        {
            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(42);
            mock.Candidate().Returns(42);
            const string experimentName = nameof(ControlLast_specified_should_run_last);

            var resultPublisher = new InMemoryResultPublisher();
            var scientist = new Scientist(resultPublisher);

            var result = scientist.Experiment<int>(experimentName, experiment =>
            {
                experiment.ThrowOnMismatches = true;
                experiment.UseCustomOrdering(SeededExperimentOrderer);
                experiment.Use(mock.Control);
                experiment.Try("candidate", mock.Candidate);
            });

            result.Should().Be(42);
            resultPublisher.Results<int>(experimentName).First().Matched.Should().BeTrue();
            Received.InOrder(() =>
            {
                mock.Received().Candidate();
                mock.Received().Control();
            });
        }

        private static int _seed = 123;

        public static IReadOnlyList<INamedBehaviour<T>> SeededExperimentOrderer<T>(IReadOnlyList<INamedBehaviour<T>> behaviours)
        {
            var random = new Random(_seed);
            return behaviours.OrderBy(_ => random.Next()).ToList();
        }
    }
}

