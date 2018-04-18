using System;
using FluentAssertions;
using Ruzzie.Common.Numerics;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics
{    
    public class PrimeHelperTests
    {
        [Theory]
        [InlineData(0,false)]
        [InlineData(1,false)]
        [InlineData(2,true)]
        [InlineData(709, true)]
        [InlineData(7199369, true)]
        [InlineData(7199368, false)]
        [InlineData(9199361, false)]
        public void IsPrime(int number, bool expected)
        {
            PrimeHelper.IsPrime(number).Should().Be(expected);
        }

        [Theory]
        [InlineData(0,3)]
        [InlineData(1,3)]
        [InlineData(2,3)]
        [InlineData(700,761)]
        [InlineData(123123, 130363)]
        [InlineData(7199368, 7199369)]
        [InlineData(9199361, 9199391)]
        [InlineData(9199391, 9199391)]
        [InlineData(Int64.MaxValue,3)]
        [InlineData(Int64.MaxValue-2,3)]
        public void GetPrime(long min, long expected)
        {
           min.GetPrime().Should().Be(expected);
        }     
    }
}
