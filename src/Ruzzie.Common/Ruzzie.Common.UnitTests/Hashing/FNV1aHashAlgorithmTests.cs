using System.Text;
using NUnit.Framework;
using Ruzzie.Common.Hashing;

namespace Ruzzie.Common.UnitTests.Hashing
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class FNV1aHashAlgorithmTests
    {
        private readonly FNV1AHashAlgorithm _hashAlgorithm = new FNV1AHashAlgorithm();

        [Test]
        public void HashCodeTest()
        {
            Assert.That(_hashAlgorithm.HashStringCaseInsensitive("0"), Is.EqualTo(837651325));
        }

        [Test]
        public void SameHashCodeForSameString()
        {
            string stringToHash = "FlashCache is tha bomb";
            int hashOne = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
            int hashTwo = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes("FlashCache is tha bomb"));
            Assert.That(hashOne, Is.EqualTo(hashTwo));
        }

        [Test]
        public void DifferentHashCodeForDifferentString()
        {
            string stringToHash = "FlashCache is tha bomb you know";
            int hashOne = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
            int hashTwo = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash.ToLower()));

            Assert.That(hashOne, Is.Not.EqualTo(hashTwo));
        }       
    }
}
