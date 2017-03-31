using System;
using System.Diagnostics;
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

        [TestFixture]
        public class HashStringCaseInsensitive
        {
            private readonly FNV1AHashAlgorithm _hashAlgorithm = new FNV1AHashAlgorithm();

            [TestCase("The Doctor", "the doctor")]
            [TestCase("the Doctor", "the doctor")]
            [TestCase("A", "a")]
            [TestCase("Ab", "aB")]
            [TestCase("AB", "AB")]
            [TestCase("1!!", "1!!")]
            [TestCase("Ω", "ω")]
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
                var defaultHash = 2166136261;
                Assert.That(_hashAlgorithm.HashStringCaseInsensitive(""), Is.EqualTo((int)defaultHash));
            }

            [Test]
            public void PerformanceTest()
            {
                SimpleRandom random = new SimpleRandom(7 * 37);
                int numberOfIterations = 100000;
                //Create random strings
                var randomStrings = new string[numberOfIterations];
                for (int i = 0; i < numberOfIterations; i++)
                {
                    //create string of random length
                    int strLength = random.Next(10, 100);
                    char[] chars = new char[strLength];
                    for (int j = 0; j < strLength; j++)
                    {
                        chars[j] = (char) random.Next(1, ushort.MaxValue + 1);
                    }
                    randomStrings[i] = new string(chars);
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < numberOfIterations; i++)
                {
                    _hashAlgorithm.HashStringCaseInsensitive(randomStrings[i]);
                }
                sw.Stop();

                Console.WriteLine("For " + numberOfIterations + " iterations. Total time of:" + sw.Elapsed.TotalSeconds + " seconds. ticks / string: " +
                                  (sw.Elapsed.Ticks / numberOfIterations));

            }

            [TestFixture]
            public class InvariantUpperCaseStringExtensionsTests
            {
                [TestCase("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
                [TestCase("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
                [TestCase("", "")]
                public void ToUpperInvariantString(string input, string expected)
                {
                    Assert.That(InvariantUpperCaseStringExtensions.ToUpperInvariant(input), Is.EqualTo(expected));
                }

                [Test]
                public void ToUpperInvariantStringNullThrowsException()
                {
                    Assert.That(()=> InvariantUpperCaseStringExtensions.ToUpperInvariant(null), Throws.ArgumentNullException);
                }
            }
        }
    }
}
