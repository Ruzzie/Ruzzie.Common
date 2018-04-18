using System;
using System.Linq;
using FluentAssertions;
using Ruzzie.Common.Numerics.Distributions;
using Ruzzie.Common.Numerics.Statistics;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics.Distributions
{
    public class ChiSquaredTests
    {        
        public class ChiSquaredPTests
        {
            [Theory]
            [InlineData(new[] {1, 2, 3}, 1)]
            [InlineData(new[] {1, 1, 1, 1, 1, 1, 1}, 7)]
            [InlineData(new[] {1, 2, 2, 2, 2, 2, 3}, 6.1428571428571423d)]
            public void ChiSquaredPWithHistogramTests(int[] numbers, double expectedChiSquared)
            {
                //Act
                double chisqP = ChiSquared.ChiSquaredP(numbers.Max(), numbers.Length, numbers.ToHistogramDictionary());

                //Assert
                chisqP.Should().Be(expectedChiSquared);
            }

            [Theory]
            [InlineData(new byte[] {1, 2, 3}, 253.0d)]
            [InlineData(new byte[] {1, 1, 1, 1, 1, 1, 1}, 1785.0d)]
            [InlineData(new byte[] {1, 2, 2, 2, 2, 2, 3}, 980.42857142857144d)]
            public void ChiSquaredPWithBytesTests(byte[] numbers, double expectedChiSquared)
            {
                //Act
                double chisqP = ChiSquared.ChiSquaredP(numbers.Length, numbers);

                //Assert
                chisqP.Should().Be(expectedChiSquared);
            }

            [Fact]
            public void ChiSquaredPWithBytesThrowsArgumentExceptionWhenSamplesLengthIsZero()
            {
                Action act = () => ChiSquared.ChiSquaredP(1, new byte[] {});
                act.Should().Throw<ArgumentException>();                
            }

            [Fact]
            public void ChiSquaredPWithBytesThrowsArgumentNullExceptionWhenSamplesIsNull()
            {
                Action act = () => ChiSquared.ChiSquaredP(1, null);
                act.Should().Throw<ArgumentNullException>();                
            }

            [Fact]
            public void ChiSquaredPWithBytesThrowsArgumentOutOfRangeExceptionWhenSampleSizeIsZero()
            {
                Action act = () => ChiSquared.ChiSquaredP(0, new byte[] {1, 2, 3});                
                act.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact]
            public void ChiSquaredPWithHistogramThrowsArgumentNullExceptionWhenHistogramIsNull()
            {
                Action act = () => ChiSquared.ChiSquaredP(1, 1, null);                
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ChiSquaredPWithHistogramThrowsArgumentOutOfRangeExceptionWhenMaxValueIsZero()
            {
                Action act = () => ChiSquared.ChiSquaredP(0, 1, new[] {1}.ToHistogramDictionary());
                act.Should().Throw<ArgumentOutOfRangeException>();                
            }

            [Fact]
            public void ChiSquaredPWithHistogramThrowsArgumentOutOfRangeExceptionWhenSampleSizeIsZero()
            {                
                Action act = () => ChiSquared.ChiSquaredP(1, 0, new[] { 1 }.ToHistogramDictionary());
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }
        
        public class PofChisquaredTests
        {
            [Theory]
            [InlineData(new[] {1, 2, 3}, 0.80125195671076077d)]
            [InlineData(new[] {1, 1, 1, 1, 1, 1, 1}, 0.0081509730909454792d)]
            [InlineData(new[] {1, 2, 2, 2, 2, 2, 3}, 0.10486303497880395d)]
            public void PofChiSquaredIntsTests(int[] numbers, double expectedPofChiSquared)
            {
                //Arrange
                int maxValue = numbers.Max();

                //Act
                double pOfChiSquared =
                    ChiSquared.ProbabilityOfChiSquared(ChiSquared.ChiSquaredP(maxValue, numbers.Length, numbers.ToHistogramDictionary()), maxValue);

                //Assert
                pOfChiSquared.Should().BeApproximately(expectedPofChiSquared, 0.0000000000000001);
            }

            [Theory]
            [InlineData(new byte[] {1, 2, 3}, 0.52362299136811341d)]
            [InlineData(new byte[] {1, 1, 1, 1, 1, 1, 1}, 0)]
            [InlineData(new byte[] {1, 2, 255, 4}, 0.54134061111835874d)]
            public void PofChiSquaredBytesTests(byte[] numbers, double expectedPofChiSquared)
            {
                //Act
                double pOfChiSquared = ChiSquared.ProbabilityOfChiSquared(ChiSquared.ChiSquaredP(numbers.Length, numbers), byte.MaxValue);

                //Assert
                pOfChiSquared.Should().Be(expectedPofChiSquared);
            }

            [Theory]
            [InlineData(0.0, 1, 1.0)]
            public void PodChiSquaredTests(double ax, int degreesOfFreedom, double expected)
            {
                ChiSquared.ProbabilityOfChiSquared(ax, degreesOfFreedom).Should().Be(expected);
            }

        
            [Fact(Skip = "takes too long")]
            public void PofChiSquaredPerformanceTest()
            {
                //Act
                double pochisq = ChiSquared.ProbabilityOfChiSquared(2147482648.3179908d, int.MaxValue);

                //Assert
                pochisq.Should().Be(0.50595716797490065d);
            }
        }

        //TODO: Calculate for larger dataset       
    }
}