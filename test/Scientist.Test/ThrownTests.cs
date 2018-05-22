using System;
using System.Linq;
using GitHub;
using NSubstitute;
using UnitTests;
using Xunit;

/// <summary>
/// Tests all of the conditions that 
/// </summary>
public class ThrownTests
{
    [Fact]
    public void CompareOperation()
    {
        const int expectedResult = 42;

        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(0);

        var ex = new Exception();

        var result = Scientist.Science<int>(nameof(CompareOperation), experiment =>
        {
            experiment.Thrown(mock.Thrown);
            experiment.Compare((x, y) =>
            {
                throw ex;
            });
            experiment.Use(mock.Control);
            experiment.Try(mock.Candidate);
        });

        Assert.Equal(expectedResult, result);
        mock.Received().Thrown(Operation.Compare, ex);
    }

    [Fact]
    public void EnabledOperation()
    {
        var settings = Substitute.For<IScientistSettings>();
        var ex = new Exception();
        settings.Enabled().Throws(ex);

        using (Swap.Enabled(settings.Enabled))
        {
            const int expectedResult = 42;

            var mock = Substitute.For<IControlCandidate<int>>();
            mock.Control().Returns(expectedResult);
            mock.Candidate().Returns(0);

            var result = Scientist.Science<int>(nameof(EnabledOperation), experiment =>
            {
                experiment.Thrown(mock.Thrown);
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(expectedResult, result);
            mock.Received().Thrown(Operation.Enabled, ex);
        }
    }

    [Fact]
    public void IgnoreOperation()
    {
        const int expectedResult = 42;

        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(0);

        var ex = new Exception();

        var result = Scientist.Science<int>(nameof(IgnoreOperation), experiment =>
        {
            experiment.Thrown(mock.Thrown);
            experiment.Ignore((x, y) =>
            {
                throw ex;
            });
            experiment.Use(mock.Control);
            experiment.Try(mock.Candidate);
        });

        Assert.Equal(expectedResult, result);
        mock.Received().Thrown(Operation.Ignore, ex);
    }

    [Fact]
    public void PublishOperation()
    {
        var publisher = Substitute.For<IResultPublisher>();
        var ex = new Exception();
        publisher.Publish(Arg.Any<Result<int, int>>()).Throws(ex);

        const int expectedResult = 42;

        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(0);

        var scientist = new Scientist(publisher);

        var result = scientist.Experiment<int>(nameof(PublishOperation), experiment =>
        {
            experiment.Thrown(mock.Thrown);
            experiment.Use(mock.Control);
            experiment.Try(mock.Candidate);
        });

        Assert.Equal(expectedResult, result);
        mock.Received().Thrown(Operation.Publish, ex);
    }

    [Fact]
    public void RunIfOperation()
    {
        const int expectedResult = 42;

        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(0);

        var ex = new Exception();

        var result = Scientist.Science<int>(nameof(RunIfOperation), experiment =>
        {
            experiment.Thrown(mock.Thrown);
            experiment.RunIf(() =>
            {
                throw ex;
            });
            experiment.Use(mock.Control);
            experiment.Try(mock.Candidate);
        });

        Assert.Equal(expectedResult, result);
        mock.Received().Thrown(Operation.RunIf, ex);
    }

    [Fact]
    public void DefaultThrow()
    {
        var mock = Substitute.For<IControlCandidate<int>>();

        var ex = new Exception();

        Action action = () => Scientist.Science<int>(nameof(DefaultThrow), experiment =>
        {
            experiment.Compare((x, y) =>
            {
                throw ex;
            });
            experiment.Use(mock.Control);
            experiment.Try(mock.Candidate);
        });

        var exception = Assert.Throws<AggregateException>(action);
        var operationException = Assert.IsType<OperationException>(exception.InnerException);
        Assert.Equal(Operation.Compare, operationException.Operation);

        var actualException = Assert.IsType<Exception>(operationException.InnerException);
        Assert.Equal(ex, actualException);
    }

    [Fact]
    public void CannotChangeStaticResultPublisherAfterAnExperiment()
    {
        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(42);
        mock.Candidate().Returns(42);
        const string experimentName = nameof(CannotChangeStaticResultPublisherAfterAnExperiment);

        var result = Scientist.Science<int>(experimentName, experiment =>
        {
            experiment.Use(mock.Control);
            experiment.Try("candidate", mock.Candidate);
        });

        Assert.Equal(42, result);
        mock.Received().Control();
        mock.Received().Candidate();
        Assert.True(TestHelper.Results<int>(experimentName).First().Matched);

        Assert.Throws<InvalidOperationException>(() => Scientist.ResultPublisher = Substitute.For<IResultPublisher>());
    }
}
