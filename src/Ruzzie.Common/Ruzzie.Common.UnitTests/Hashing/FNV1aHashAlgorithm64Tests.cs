using System;
using System.Text;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing;

// ReSharper disable once InconsistentNaming
public class FNV1aHashAlgorithm64Tests
{
    private readonly FNV1AHashAlgorithm64 _hashAlgorithm = new FNV1AHashAlgorithm64();

    [Fact]
    public void HashCodeTest()
    {
        _hashAlgorithm.HashStringCaseInsensitive("0").Should().Be(575378865958763869);
    }

    [Fact]
    public void SameHashCodeForSameString()
    {
        string stringToHash = "FlashCache is tha bomb";
        long   hashOne      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
        long   hashTwo      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes("FlashCache is tha bomb"));
        hashOne.Should().Be(hashTwo);
    }

    [Fact]
    public void DifferentHashCodeForDifferentString()
    {
        string stringToHash = "FlashCache is tha bomb";
        long   hashOne      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
        long   hashTwo      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash.ToLower()));

        hashOne.Should().NotBe(hashTwo);
    }


    [Theory]
    [InlineData("Enchantment", "Human")]
    [InlineData("Human",       "Wizard")]
    public void DifferentHashCodesTest(string a, string b)
    {
        long hashOne = _hashAlgorithm.HashStringCaseInsensitive(a);
        long hashTwo = _hashAlgorithm.HashStringCaseInsensitive(b);

        hashOne.Should().NotBe(hashTwo);
    }

    public class HashStringCaseInsensitive
    {
        private readonly FNV1AHashAlgorithm64 _hashAlgorithm = new FNV1AHashAlgorithm64();

        [Theory]
        [InlineData("The Doctor",       "the doctor")]
        [InlineData("the Doctor",       "the doctor")]
        [InlineData("A",                "a")]
        [InlineData("1!!",              "1!!")]
        [InlineData("Ω",                "ω")]
        [InlineData("3 Harvard Square", "3 HARVARD SQUARE")]
        public void IgnoreCaseTests(string casingOne, string casingStyleTwo)
        {
            _hashAlgorithm.HashStringCaseInsensitive(casingOne)
                          .Should()
                          .Be(_hashAlgorithm.HashStringCaseInsensitive(casingStyleTwo));
        }

        [Fact]
        public void NullShouldNotThrowException()
        {
            _hashAlgorithm.HashStringCaseInsensitive(null).Should().Be(-3750763034362895579L);
        }

        [Fact]
        public void EmptyShouldReturnDefaultHashValue()
        {
            var defaultHash = -3750763034362895579L;
            _hashAlgorithm.HashStringCaseInsensitive("").Should().Be(defaultHash);
        }
    }
}