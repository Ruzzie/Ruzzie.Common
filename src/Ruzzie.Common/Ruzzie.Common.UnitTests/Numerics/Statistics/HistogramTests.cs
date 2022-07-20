using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ruzzie.Common.Numerics.Statistics;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics;

public class HistogramTests
{
#if !NET40
    [Theory]
    [InlineData(new[] {0, 1, 1, 1, 2}, 1,            3)]
    [InlineData(new[] {0, 1, 1, 1, 2}, 0,            1)]
    [InlineData(new[] {0, 1, 1, 1, 2}, 2,            1)]
    [InlineData(new[] {0},             0,            1)]
    [InlineData(new[] {-1},            -1,           1)]
    [InlineData(new[] {int.MaxValue},  int.MaxValue, 1)]
    [InlineData(new[] {int.MinValue},  int.MinValue, 1)]
    public void ToHistogramDictionaryIntsTest(int[] values, int keyToAssert, int expectedCount)
    {
        //Act
        IDictionary<int, int> histogram = values.ToHistogramDictionary();

        //Assert
        histogram[keyToAssert].Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(new[] {0, 1, 1, 1, 2}, 1,            3)]
    [InlineData(new[] {0, 1, 1, 1, 2}, 0,            1)]
    [InlineData(new[] {0, 1, 1, 1, 2}, 2,            1)]
    [InlineData(new[] {0},             0,            1)]
    [InlineData(new[] {-1},            -1,           1)]
    [InlineData(new[] {int.MaxValue},  int.MaxValue, 1)]
    [InlineData(new[] {int.MinValue},  int.MinValue, 1)]
    public void ToHistogramOrderedIntsTests(int[] values, int keyToAssert, int expectedCount)
    {
        //Act
        IOrderedEnumerable<KeyValuePair<int, int>> histogram = values.ToHistogramOrdered();

        //Assert
        histogram.First(kvp => kvp.Key == keyToAssert).Value.Should().Be(expectedCount);
    }

    [Fact]
    public void ToHistogramDictionaryThrowsArgumentNullExceptionWhenValuesIsNull()
    {
        Action act = () => ((int[]) null).ToHistogramDictionary();            
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHistogramEnumerableThrowsArgumentNullExceptionWhenValuesIsNull()
    {            
        Action act = () => ((int[])null).ToHistogramOrdered();
        act.Should().Throw<ArgumentNullException>();
    }
#endif
    [Fact]
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
        histogram.Should().BeEquivalentTo(expected);
    }
}