using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ruzzie.Common.Numerics.Statistics;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics
{
    [TestFixture]
    public class HistogramTests
    {
        [TestCase(new[] {0, 1, 1, 1, 2}, 1, 3)]
        [TestCase(new[] {0, 1, 1, 1, 2}, 0, 1)]
        [TestCase(new[] {0, 1, 1, 1, 2}, 2, 1)]
        [TestCase(new[] {0}, 0, 1)]
        [TestCase(new[] {-1}, -1, 1)]
        [TestCase(new[] {int.MaxValue}, int.MaxValue, 1)]
        [TestCase(new[] {int.MinValue}, int.MinValue, 1)]
        public void ToHistogramDictionaryIntsTest(int[] values, int keyToAssert, int expectedCount)
        {
            //Act
            IDictionary<int, int> histogram = values.ToHistogramDictionary();

            //Assert
            Assert.That(histogram[keyToAssert], Is.EqualTo(expectedCount));
        }

        [TestCase(new[] {0, 1, 1, 1, 2}, 1, 3)]
        [TestCase(new[] {0, 1, 1, 1, 2}, 0, 1)]
        [TestCase(new[] {0, 1, 1, 1, 2}, 2, 1)]
        [TestCase(new[] {0}, 0, 1)]
        [TestCase(new[] {-1}, -1, 1)]
        [TestCase(new[] {int.MaxValue}, int.MaxValue, 1)]
        [TestCase(new[] {int.MinValue}, int.MinValue, 1)]
        public void ToHistogramOrderedIntsTests(int[] values, int keyToAssert, int expectedCount)
        {
            //Act
            IOrderedEnumerable<KeyValuePair<int, int>> histogram = values.ToHistogramOrdered();

            //Assert
            Assert.That(histogram.First(kvp => kvp.Key == keyToAssert).Value, Is.EqualTo(expectedCount));
        }

        [Test]
        public void ToHistogramDictionaryThrowsArgumentNullExceptionWhenValuesIsNull()
        {
            Assert.That(() => ((int[]) null).ToHistogramDictionary(), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ToHistogramEnumerableThrowsArgumentNullExceptionWhenValuesIsNull()
        {
            Assert.That(() => ((int[])null).ToHistogramOrdered(), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ToToHistogramOrderedOrderingTest()
        {
            //Arrange
            int[] values = {10, 1, 2, 3, 1, 1, 3, 3, 10, 50, 3, 2};
            IEnumerable<KeyValuePair<int, int>> expected = new[]
            {
                new KeyValuePair<int, int>(1, 3), new KeyValuePair<int, int>(2, 2), new KeyValuePair<int, int>(3, 4),
                new KeyValuePair<int, int>(10, 2), new KeyValuePair<int, int>(50, 1)
            };

            //Act
            IOrderedEnumerable<KeyValuePair<int, int>> histogram = values.ToHistogramOrdered();

            //Assert
            Assert.That(histogram, Is.EqualTo(expected));
        }
    }
}