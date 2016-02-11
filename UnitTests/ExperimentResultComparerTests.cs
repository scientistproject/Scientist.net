using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub;
using GitHub.Internals;
using NSubstitute;
using Xunit;

namespace UnitTests
{
    public class ExperimentResultComparerTests
    {
        [Fact]
        public void BothResultsAreBaseClassEquals()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void BothResultsAreNull_AreEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<string>(null, null);
            var controlresult = new ExperimentInstance<string>.ExperimentResult((string)null, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<string>.ExperimentResult((string)null, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void BothResultsAreNotIEquatable_AndSame_AreEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<NotIEquatable>(null, null);
            var controlresult = new ExperimentInstance<NotIEquatable>.ExperimentResult(new NotIEquatable("42"), TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<NotIEquatable>.ExperimentResult(new NotIEquatable("42"), TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void CandadateResult_IsNull_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(null, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }

        [Fact]
        public void ControlResult_IsNull_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(null, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }

        [Fact]
        public void ControlAndCandidateAreDiffrent_But_ComparisonFunction_says_AreEqual()
        {
            //Arrange
            Func<int, int, bool> equalsFunction = (control, candidate) => true;

            var comparer = new ExperimentResultComparer<int>(null, equalsFunction);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(0, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void ControlAndCandidateAreComplexObjects_AndImplement_IEqualitable_AreEqual()
        {
            //Arrange
            IEquatable<ComplexResult> controlComplexResult = new ComplexResult() { Count = 42, Name = "42" };
            IEquatable<ComplexResult> candidateComplexResult = new ComplexResult() { Count = 42, Name = "42" };

            var comparer = new ExperimentResultComparer<ComplexResult>(null, null);
            var controlresult = new ExperimentInstance<ComplexResult>.ExperimentResult((ComplexResult)controlComplexResult, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<ComplexResult>.ExperimentResult((ComplexResult)candidateComplexResult, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void ControlAndCandidateAreDiffrent_But_IEqualityComparer_says_AreEqual()
        {
            //Arrange
            var mock = Substitute.For<IEqualityComparer<int>>();
            mock.Equals(0, 42).Returns(true);

            var comparer = new ExperimentResultComparer<int>(mock, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(0, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void ControlHasNormalResult_CandidateThrowsException_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(0, TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }

        [Fact]
        public void ControlThrowsException_CandidateThrowsException_AreEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.True(areEqual);
        }

        public void ControlThrowsException_CandidateHasNormalResult_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(42, TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }

        [Fact]
        public void Control_And_CandidateThrowsException_WithDiffrentMessages_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(new Exception("bang!"), TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }

        [Fact]
        public void Control_And_CandidateThrowsException_WithDiffrentTypes_AreNotEqual()
        {
            //Arrange
            var comparer = new ExperimentResultComparer<int>(null, null);
            var controlresult = new ExperimentInstance<int>.ExperimentResult(new InvalidOperationException("boom!"), TimeSpan.FromMilliseconds(1));
            var candidateresult = new ExperimentInstance<int>.ExperimentResult(new Exception("boom!"), TimeSpan.FromMilliseconds(1));

            //Act
            bool areEqual = comparer.Equals(controlresult, candidateresult);

            //Assert
            Assert.False(areEqual);
        }
    }

    /// <summary>
    /// Test class with override Equals() method but does not implement IEquatable
    /// </summary>
    public class NotIEquatable
    {
        public NotIEquatable(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public override bool Equals(object obj)
        {
            NotIEquatable other = obj as NotIEquatable;

            return other != null && Name.Equals(other.Name, StringComparison.Ordinal);
        }

    }
}
