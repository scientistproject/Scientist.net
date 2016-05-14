using GitHub;
using GitHub.Internals;
using NSubstitute;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

/// <summary>
/// Tests all science components that involve <see cref="ILaboratory"/>
/// </summary>
public class LaboratoryTests
{
    [Fact]
    public async Task DefaultLaboratoryIsEnabled()
    {
        var target = new DefaultLaboratory();
        bool actual = await target.Enabled();
        Assert.True(actual);
    }

    [Fact]
    public void LaboratoryDisablesExperiment()
    {
        const int expectedResult = 42;

        var mock = Substitute.For<IControlCandidate<int>>();
        mock.Control().Returns(expectedResult);
        mock.Candidate().Returns(0);

        ILaboratory laboratory = Substitute.For<ILaboratory>();
        laboratory.Enabled().Returns(false);
        using (Swap.Laboratory(laboratory))
        {
            var result = Scientist.Science<int>(nameof(LaboratoryDisablesExperiment), experiment =>
            {
                experiment.Use(mock.Control);
                experiment.Try(mock.Candidate);
            });

            Assert.Equal(expectedResult, result);
            mock.DidNotReceive().Candidate();
            mock.Received().Control();
        }
    }
}