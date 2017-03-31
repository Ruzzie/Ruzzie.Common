using NUnit.Framework;
using Ruzzie.Common.Hashing;

namespace Ruzzie.Common.UnitTests.Hashing
{
    [TestFixture]
    public class InvariantUpperCaseStringExtensionsTests
    {
        [TestCase("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("", "")]
        [TestCase("3 Harvard Square", "3 HARVARD SQUARE")]
        [TestCase("2130 South Fort Union Blvd.", "2130 SOUTH FORT UNION BLVD.")]
        public void ToUpperInvariantString(string input, string expected)
        {
            Assert.That(InvariantUpperCaseStringExtensions.ToUpperInvariant(input), Is.EqualTo(expected));
        }

        [Test]
        public void ToUpperInvariantStringNullThrowsException()
        {
            Assert.That(() => InvariantUpperCaseStringExtensions.ToUpperInvariant(null), Throws.ArgumentNullException);
        }
    }
}