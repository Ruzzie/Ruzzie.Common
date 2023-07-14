using System;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing;

public class ValueTypeFNV1AHashCodeTests
{
    [Fact]
    public void Float()
    {
        float x = 1.233346f;

        var myHash = ValueTypeFNV1AHashCode<float>.HashCode(ref x);
        var bcHash = FNV1AHashAlgorithm.Hash(BitConverter.GetBytes(x));


        myHash.Should().Be(-1071519449);
        myHash.Should().Be(bcHash);
    }
}