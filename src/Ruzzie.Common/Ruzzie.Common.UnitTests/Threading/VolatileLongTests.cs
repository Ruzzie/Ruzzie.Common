using FluentAssertions;
using Ruzzie.Common.Threading;
using Xunit;

namespace Ruzzie.Common.UnitTests.Threading
{    
    public class VolatileLongTests
    {
        [Fact]
        public void SmokeTest()
        {
            VolatileLong index = 1L;

            index.ReadUnfenced().Should().Be(1);
        }

        [Fact]
        public void VolatileValue()
        {
            VolatileLong index = 1L;
            index.VolatileValue = 2L;

            index.VolatileValue.Should().Be(2);
        }

        [Fact]
        public void CompilerFencedValue()
        {
            VolatileLong index = 1L;
            index.CompilerFencedValue = 2L;

            index.CompilerFencedValue.Should().Be(2);
        }

        [Fact]
        public void AtomicCompareExchange()
        {
            VolatileLong index = 1L;

            index.AtomicCompareExchange(5L, 1L).Should().BeTrue();            
            index.ReadUnfenced().Should().Be(5);
        }

        [Fact]
        public void AtomicIncrement()
        {
            long initialValue = 1L;
            VolatileLong index = initialValue;

            index.AtomicIncrement().Should().Be(2L);
            index.ReadUnfenced().Should().Be(2L);
        }

        [Fact]
        public void AtomicIncrementOverflowTest()
        {
            VolatileLong index = long.MaxValue;

            index.AtomicIncrement();

            index.ReadUnfenced().Should().Be(long.MinValue);
        }

        [Fact]
        public void AtomicDecrement()
        {
            long initialValue = 1L;
            VolatileLong index = initialValue;

            index.AtomicDecrement().Should().Be(0L);
            index.ReadUnfenced().Should().Be(0L);
        }

        [Fact]
        public void AtomicDecrementUnderflowTest()
        {
            VolatileLong index = long.MinValue;

            index.AtomicDecrement();

            index.ReadUnfenced().Should().Be(long.MaxValue);
        }
    }
}
