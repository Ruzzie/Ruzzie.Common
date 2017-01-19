using NUnit.Framework;
using Ruzzie.Common.Threading;

namespace Ruzzie.Common.UnitTests.Threading
{
    [TestFixture]
    public class VolatileLongTests
    {
        [Test]
        public void SmokeTest()
        {
            VolatileLong index = 1L;

            Assert.That(index.ReadUnfenced(), Is.EqualTo(1));
        }

        [Test]
        public void VolatileValue()
        {
            VolatileLong index = 1L;
            index.VolatileValue = 2L;

            Assert.That(index.VolatileValue, Is.EqualTo(2));
        }

        [Test]
        public void CompilerFencedValue()
        {
            VolatileLong index = 1L;
            index.CompilerFencedValue = 2L;

            Assert.That(index.CompilerFencedValue, Is.EqualTo(2));
        }

        [Test]
        public void AtomicCompareExchange()
        {
            VolatileLong index = 1L;

            Assert.That(index.AtomicCompareExchange(5L, 1L), Is.True);
            Assert.That(index.ReadUnfenced(), Is.EqualTo(5));
        }

        [Test]
        public void AtomicIncrement()
        {
            long initialValue = 1L;
            VolatileLong index = initialValue;

            Assert.That(index.AtomicIncrement(), Is.EqualTo(2L));
            Assert.That(index.ReadUnfenced(), Is.EqualTo(2L));
        }

        [Test]
        public void AtomicIncrementOverflowTest()
        {
            VolatileLong index = long.MaxValue;

            index.AtomicIncrement();

            Assert.That(index.ReadUnfenced(),Is.EqualTo(long.MinValue));
        }
    }
}
