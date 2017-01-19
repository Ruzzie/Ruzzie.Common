using System;
using System.Linq;
using NUnit.Framework;
using Ruzzie.Common.Numerics.Distributions;
using Ruzzie.Common.Numerics.Statistics;

namespace Ruzzie.Common.UnitTests.Numerics.Distributions
{
    [TestFixture]
    public class ChiSquaredTests
    {
        [TestFixture]
        public class ChiSquaredPTests
        {
            [TestCase(new[] {1, 2, 3}, 1)]
            [TestCase(new[] {1, 1, 1, 1, 1, 1, 1}, 7)]
            [TestCase(new[] {1, 2, 2, 2, 2, 2, 3}, 6.1428571428571423d)]
            public void ChiSquaredPWithHistogramTests(int[] numbers, double expectedChiSquared)
            {
                //Act
                double chisqP = ChiSquared.ChiSquaredP(numbers.Max(), numbers.Length, numbers.ToHistogramDictionary());

                //Assert
                Assert.That(chisqP, Is.EqualTo(expectedChiSquared));
            }

            [TestCase(new byte[] {1, 2, 3}, 253.0d)]
            [TestCase(new byte[] {1, 1, 1, 1, 1, 1, 1}, 1785.0d)]
            [TestCase(new byte[] {1, 2, 2, 2, 2, 2, 3}, 980.42857142857144d)]
            public void ChiSquaredPWithBytesTests(byte[] numbers, double expectedChiSquared)
            {
                //Act
                double chisqP = ChiSquared.ChiSquaredP(numbers.Length, numbers);

                //Assert
                Assert.That(chisqP, Is.EqualTo(expectedChiSquared));
            }

            [Test]
            public void ChiSquaredPWithBytesThrowsArgumentExceptionWhenSamplesLengthIsZero()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(1, new byte[] {}), Throws.Exception.TypeOf<ArgumentException>());
            }

            [Test]
            public void ChiSquaredPWithBytesThrowsArgumentNullExceptionWhenSamplesIsNull()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(1, null), Throws.Exception.TypeOf<ArgumentNullException>());
            }

            [Test]
            public void ChiSquaredPWithBytesThrowsArgumentOutOfRangeExceptionWhenSampleSizeIsZero()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(0, new byte[] {1, 2, 3}), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }

            [Test]
            public void ChiSquaredPWithHistogramThrowsArgumentNullExceptionWhenHistogramIsNull()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(1, 1, null), Throws.Exception.TypeOf<ArgumentNullException>());
            }

            [Test]
            public void ChiSquaredPWithHistogramThrowsArgumentOutOfRangeExceptionWhenMaxValueIsZero()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(0, 1, new[] {1}.ToHistogramDictionary()),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }

            [Test]
            public void ChiSquaredPWithHistogramThrowsArgumentOutOfRangeExceptionWhenSampleSizeIsZero()
            {
                Assert.That(() => ChiSquared.ChiSquaredP(1, 0, new[] {1}.ToHistogramDictionary()),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [TestFixture]
        public class PofChisquaredTests
        {
            [TestCase(new[] {1, 2, 3}, 0.80125195671076077d)]
            [TestCase(new[] {1, 1, 1, 1, 1, 1, 1}, 0.0081509730909454792d)]
            [TestCase(new[] {1, 2, 2, 2, 2, 2, 3}, 0.10486303497880395d)]
            public void PofChiSquaredIntsTests(int[] numbers, double expectedPofChiSquared)
            {
                //Arrange
                int maxValue = numbers.Max();

                //Act
                double pOfChiSquared =
                    ChiSquared.ProbabilityOfChiSquared(ChiSquared.ChiSquaredP(maxValue, numbers.Length, numbers.ToHistogramDictionary()), maxValue);

                //Assert
                Assert.That(pOfChiSquared, Is.EqualTo(expectedPofChiSquared));
            }

            [TestCase(new byte[] {1, 2, 3}, 0.52362299136811341d)]
            [TestCase(new byte[] {1, 1, 1, 1, 1, 1, 1}, 0)]
            [TestCase(new byte[] {1, 2, 255, 4}, 0.54134061111835874d)]
            public void PofChiSquaredBytesTests(byte[] numbers, double expectedPofChiSquared)
            {
                //Act
                double pOfChiSquared = ChiSquared.ProbabilityOfChiSquared(ChiSquared.ChiSquaredP(numbers.Length, numbers), byte.MaxValue);

                //Assert
                Assert.That(pOfChiSquared, Is.EqualTo(expectedPofChiSquared));
            }

            [TestCase(0.0, 1, 1.0)]
            public void PodChiSquaredTests(double ax, int degreesOfFreedom, double expected)
            {
                Assert.That(ChiSquared.ProbabilityOfChiSquared(ax, degreesOfFreedom), Is.EqualTo(expected));
            }

            [Test]
            [Ignore("takes too long")]
            public void PofChiSquaredPerformanceTest()
            {
                //Act
                double pochisq = ChiSquared.ProbabilityOfChiSquared(2147482648.3179908d, int.MaxValue);

                //Assert
                Assert.That(pochisq, Is.EqualTo(0.50595716797490065d));
            }
        }

        //TODO: Calculate for larger dataset       
    }
}