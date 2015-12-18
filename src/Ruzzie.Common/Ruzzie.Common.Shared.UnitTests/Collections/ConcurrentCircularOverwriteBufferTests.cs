using NUnit.Framework;

namespace Ruzzie.Common.Collections.Tests
{
    [TestFixture]
    public class ConcurrentCircularOverwriteBufferTests
    {
        [TestCase(1)]
        [TestCase(0)]
        [TestCase(-1)]
        public void ConstructorThrowsArgumentExceptionWhenSizeIsLessThanTwo(int size)
        {
            Assert.That(() => new ConcurrentCircularOverwriteBuffer<int>(size), Throws.Exception);
        }

        [Test]
        public void CountShouldNotExceedCapacity()
        {
            var size = 2;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);

            Assert.That(buffer.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReadNextThrowsExceptionWhenEmpty()
        {
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

            Assert.That(() => buffer.ReadNext(), Throws.Exception);
        }

        [Test]
        public void SmokeTest()
        {
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>();

            buffer.WriteNext(1);
            buffer.WriteNext(2);

            Assert.That(buffer.ReadNext(), Is.EqualTo(1));
            Assert.That(buffer.ReadNext(), Is.EqualTo(2));
        }

        [Test]
        public void WriteNextShouldOverwriteValues()
        {
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

            for (var i = 0; i < 10; i++)
            {
                buffer.WriteNext(i);
            }

            Assert.That(buffer.ReadNext(), Is.EqualTo(8));
            Assert.That(buffer.ReadNext(), Is.EqualTo(9));
        }
    }
}