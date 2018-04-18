using System;
using System.Collections.Generic;
using FluentAssertions;
using Ruzzie.Common.Numerics.Statistics;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics
{    
    public class EntropyTests
    {
        [Theory]
        [InlineData(new[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 3.5849625007211561d)]
        [InlineData(new[] { 1, 2, 3, 3, 2, 2, 1, 1, 3 }, 1.5849625007211561d)]
        [InlineData(new[] { 0 }, 0)]
        public void Test(int[] numbers, double expectedEntropy)
        {
            //Act
            double entropy = numbers.ToHistogramDictionary().CalculateEntropy(numbers.Length);
                
            //Assert            
            entropy.Should().Be(expectedEntropy);
        }

        [Fact]
        public void CalculateEntropyWithDictionaryThrowsArgumentNullExceptionsWhenHistogramIsNull()
        {
            Action act = ()=> ((IDictionary<int, int>) null).CalculateEntropy(1);
            act.Should().Throw<ArgumentNullException>();            
        }

        [Fact]
        public void CalculateEntropyWithIEnumerableThrowsArgumentNullExceptionsWhenHistogramIsNull()
        {            
            Action act = () => ((IEnumerable<KeyValuePair<int, int>>)null).CalculateEntropy(1);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}