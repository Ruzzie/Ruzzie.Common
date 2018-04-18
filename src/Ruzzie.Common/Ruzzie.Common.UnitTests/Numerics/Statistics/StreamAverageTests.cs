using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics
{    
    public class StreamAverageTests
    {
        [Theory]
        [InlineData(new double[] { 1, 1, 1 }, 1.0)]
        [InlineData(new double[] { 1, 1, 1, 2, 2, 2 }, 1.5)]
        [InlineData(new double[] { 1, 2 }, 1.5)]
        [InlineData(new double[] { 0, 9 }, 4.5)]
        [InlineData(new double[] { 0, 10, 0, 10, 0, 10 }, 5)]
        public void DoubleTests(double[] items, double expected)
        {
            double avg = 0;
            for (int i = 0; i < items.Length; i++)
            {
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, items[i], i);
            }

            avg.Should().Be(expected);
            avg.Should().Be(items.Average());
        }

        [Theory]
        [InlineData(new[] { 1, 1, 1 }, 1.0)]
        [InlineData(new[] { 1, 1, 1, 2, 2, 2 }, 1.5)]
        [InlineData(new[] { 1, 2 }, 1.5)]
        [InlineData(new[] { 0, 9 }, 4.5)]
        [InlineData(new[] { 0, 10, 0, 10, 0, 10 }, 5)]
        [InlineData(new[] { 1, 2, 3, 1, 2, 4 }, 2.1666666666666665)]
        public void StreamAverageIntTests(int[] items, double expected)
        {
            double avg = 0;
            for (int i = 0; i < items.Length; i++)
            {
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, items[i], i);
            }

            avg.Should().Be(expected);
            avg.Should().Be(items.Average());
        }

        [Fact]
        public void StreamAverageForSetTest()
        {
            Random random = new Random(1);
            double avg = 0;
            List<int> samples = new List<int>(1000);
            for (int i = 0; i < 1000; i++)
            {
                int value = random.Next();
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, value, i);
                samples.Add(value);
            }

            avg.Should().Be(1081420669.4020009d);
            avg.Should().BeApproximately(samples.Average(), 0.000009);
        }
    }
}