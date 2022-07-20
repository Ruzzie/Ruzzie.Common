using System;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing;

public class InvariantUpperCaseStringExtensionsTests
{
    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyz",  "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("",                            "")]
    [InlineData("3 Harvard Square",            "3 HARVARD SQUARE")]
    [InlineData("2130 South Fort Union Blvd.", "2130 SOUTH FORT UNION BLVD.")]
    public void ToUpperInvariantString(string input, string expected)
    {
        InvariantUpperCaseStringExtensions.ToUpperInvariant(input).Should().Be(expected);
    }

    [Fact]
    public void ToUpperInvariantStringNullThrowsException()
    {
        Action act = () => InvariantUpperCaseStringExtensions.ToUpperInvariant(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnsafeBufferToUpperCase()
    {
        string original = "The European hare (Lepus europaeus)";
        unsafe
        {
            fixed (char* buffer = original)
            {
                InvariantUpperCaseStringExtensions.ToUpperInvariant(buffer, 0, original.Length);

                new string(buffer).Should().Be("THE EUROPEAN HARE (LEPUS EUROPAEUS)");
            }
        }
    }

    [Fact]
    public void BufferToUpperCase()
    {
        string original = "The European hare (Lepus europaeus)";
        var    buffer   = original.ToCharArray();

        InvariantUpperCaseStringExtensions.ToUpperInvariant(buffer, 0, original.Length);

        new string(buffer).Should().Be("THE EUROPEAN HARE (LEPUS EUROPAEUS)");
    }
}