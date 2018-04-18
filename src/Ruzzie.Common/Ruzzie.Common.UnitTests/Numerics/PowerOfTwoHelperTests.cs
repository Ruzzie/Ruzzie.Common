using System;
using FluentAssertions;
using Ruzzie.Common.Numerics;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics
{    
    public class PowerOfTwoHelperTests
    {
        [Theory]
        [InlineData(2, 2)]
        [InlineData(250, 256)]
        [InlineData(100, 128)]
        [InlineData(1000, 1024)]
        [InlineData(1024, 1024)]
        [InlineData(1500, 2048)]
        [InlineData(60000, 65536)]
        [InlineData(100000, 131072)]
        [InlineData(1048570, 1048576)]
        [InlineData(4194000, 4194304)]
        [InlineData(1073741800, 1073741824)]
        public void FindNearestPowerOfTwoForGivenValue(int value, int expected)
        {
            value.FindNearestPowerOfTwoEqualOrGreaterThan().Should().Be(expected);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(5, 4)]
        [InlineData(300, 256)]
        [InlineData(140, 128)]
        [InlineData(1050, 1024)]
        [InlineData(1024, 1024)]
        [InlineData(3000, 2048)]
        [InlineData(70000, 65536)]
        [InlineData(140000, 131072)]
        [InlineData(1148570, 1048576)]
        [InlineData(4494000, 4194304)]
        [InlineData(1273741800, 1073741824)]
        public void FindNearestPowerOfTwoLessThanForGivenValue(int value, int expected)
        {
            value.FindNearestPowerOfTwoEqualOrLessThan().Should().Be(expected);
        }

        [Fact]
        public void FindNearestPowerOfTwoThrowsArgumentExceptionWhenTargetValueWouldBegreaterThanMaxInt32()
        {
            Action act = () => (int.MaxValue - 1).FindNearestPowerOfTwoEqualOrGreaterThan();
            act.Should().Throw<ArgumentOutOfRangeException>();            
        }

        [Fact]
        public void FindNearestPowerOfTwoThrowsArgumentExceptionWhenTargetValueIsLessThan0()
        {
            Action act = () => (-100).FindNearestPowerOfTwoEqualOrGreaterThan();
            act.Should().Throw<ArgumentOutOfRangeException>();            
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void FindNearestPowerOfTwoEqualOrLessThanThrowsArgumentExceptionWhenValueIsLessThanOrEqualZero(int value)
        {
            Action act = () => value.FindNearestPowerOfTwoEqualOrLessThan();
            act.Should().Throw<ArgumentOutOfRangeException>();            
        }

        [Theory]
        [InlineData(2,true)]
        [InlineData(3, false)]
        [InlineData(1024, true)]
        [InlineData(999, false)]
        [InlineData(1073741824, true)]
        [InlineData(2073741824, false)]
        public void IsPowerOfTwoTests(long value, bool expected)
        {
            value.IsPowerOfTwo().Should().Be(expected);
        }
    }
}
