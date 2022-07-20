using System.Text;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing;

// ReSharper disable once InconsistentNaming
public class FNV1aHashAlgorithmTests
{
    private readonly FNV1AHashAlgorithm _hashAlgorithm = new FNV1AHashAlgorithm();

    [Fact]
    public void HashCodeTest()
    {
        _hashAlgorithm.HashStringCaseInsensitive("0").Should().Be(837651325);
    }

    [Fact]
    public void SameHashCodeForSameString()
    {
        string stringToHash = "FlashCache is tha bomb";
        int    hashOne      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
        int    hashTwo      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes("FlashCache is tha bomb"));
        hashOne.Should().Be(hashTwo);
    }

    [Fact]
    public void DifferentHashCodeForDifferentString()
    {
        string stringToHash = "FlashCache is tha bomb you know";
        int    hashOne      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash));
        int    hashTwo      = _hashAlgorithm.HashBytes(Encoding.Unicode.GetBytes(stringToHash.ToLower()));
            
        hashOne.Should().NotBe(hashTwo);
    }       
}