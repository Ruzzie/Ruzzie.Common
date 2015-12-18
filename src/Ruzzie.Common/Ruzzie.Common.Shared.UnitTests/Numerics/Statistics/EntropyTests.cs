using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruzzie.Common.Numerics.Statistics;

namespace Ruzzie.Common.Shared.UnitTests.Numerics.Statistics
{
    [TestFixture]
    public class EntropyTests
    {
        [TestCase(new[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 3.5849625007211561d)]
        [TestCase(new[] { 1, 2, 3, 3, 2, 2, 1, 1, 3 }, 1.5849625007211561d)]
        [TestCase(new[] { 0 }, 0)]
        public void Test(int[] numbers, double expectedEntropy)
        {
            //Act
            double entropy = numbers.ToHistogramDictionary().CalculateEntropy(numbers.Length);
                
            //Assert
            Assert.That(entropy, Is.EqualTo(expectedEntropy));
        }

        [Test]
        public void CalculateEntropyWithDictionaryThrowsArgumentNullExceptionsWhenHistogramIsNull()
        {            
            Assert.That(()=> ((IDictionary<int, int>) null).CalculateEntropy(1), Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CalculateEntropyWithIEnumerableThrowsArgumentNullExceptionsWhenHistogramIsNull()
        {
            Assert.That(() => ((IEnumerable<KeyValuePair<int, int>>)null).CalculateEntropy(1), Throws.Exception.TypeOf<ArgumentNullException>());
        }
    }
}