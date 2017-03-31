using System.Text;
using NUnit.Framework;
using Ruzzie.Common.Hashing;

namespace Ruzzie.Common.UnitTests.Hashing
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class FNV1aHashAlgorithm64Tests
    {
        private readonly FNV1AHashAlgorithm64 _hashAlgorithm = new FNV1AHashAlgorithm64();

        [Test]
        public void HashCodeTest()
        {
            Assert.That(_hashAlgorithm.HashStringCaseInsensitive("0"), Is.EqualTo(575378865958763869));
        }

        [Test]
        public void SameHashCodeForSameString()
        {
            string stringToHash = "FlashCache is tha bomb";
            long hashOne = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
            long hashTwo = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes("FlashCache is tha bomb"));
            Assert.That(hashOne, Is.EqualTo(hashTwo));
        }

        [Test]
        public void DifferentHashCodeForDifferentString()
        {
            string stringToHash = "FlashCache is tha bomb you know";
            long hashOne = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
            long hashTwo = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash.ToLower()));

            Assert.That(hashOne, Is.Not.EqualTo(hashTwo));
        }

        [TestFixture]
        public class HashStringCaseInsensitive
        {
            private readonly FNV1AHashAlgorithm64 _hashAlgorithm = new FNV1AHashAlgorithm64();

            [TestCase("The Doctor", "the doctor")]
            [TestCase("the Doctor", "the doctor")]
            [TestCase("A", "a")]
            [TestCase("1!!", "1!!")]
            [TestCase("Ω", "ω")]
            [TestCase("3 Harvard Square", "3 HARVARD SQUARE")]
            public void IgnoreCaseTests(string casingOne, string casingStyleTwo)
            {
                Assert.That(_hashAlgorithm.HashStringCaseInsensitive(casingOne), Is.EqualTo(_hashAlgorithm.HashStringCaseInsensitive(casingStyleTwo)));
            }

            [Test]
            public void NullShouldThrowException()
            {
                Assert.That(() => _hashAlgorithm.HashStringCaseInsensitive(null), Throws.Exception);
            }

            [Test]
            public void EmptyShouldReturnDefaultHashValue()
            {
                var defaultHash = 14695981039346656037;
                Assert.That(_hashAlgorithm.HashStringCaseInsensitive(""), Is.EqualTo((long)defaultHash));
            }
        }
    }
}
