using NUnit.Framework;

namespace Ruzzie.Common.UnitTests
{
    [TestFixture]
    public class RandomExtensionsTests
    {
        [Test]
        public void NextBytesThrowsExceptionWhenRandomIsNull()
        {
            Assert.That(() => RandomExtensions.NextBytes(null, 1), Throws.Exception);
        }

        [Test]
        public void NextBytesThrowsExceptionWhenCountIsLessThanOne()
        {
            Assert.That(() => RandomExtensions.NextBytes(new SimpleRandom(), 0), Throws.Exception);
        }
    }
}
