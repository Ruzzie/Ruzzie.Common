using System;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing
{
    public class InvariantLowerCaseStringExtensionsTests
    {
        [Theory]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ","abcdefghijklmnopqrstuvwxyz")]
        [InlineData("", "")]
        [InlineData("3 HARVARD SQUARE", "3 harvard square")]
        [InlineData("2130 South Fort Union Blvd.", "2130 south fort union blvd.")]
        public void ToUpperInvariantString(string input, string expected)
        {
            InvariantLowerCaseStringExtensions.ToLowerInvariant(input).Should().Be(expected);
        }

        [Fact]
        public void ToUpperInvariantStringNullThrowsException()
        {
            Action act = () => InvariantLowerCaseStringExtensions.ToLowerInvariant(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void UnsafeBufferToLowerCase()
        {
            string original = "The European hare (Lepus europaeus)";
            unsafe
            {
                fixed (char* buffer = original)
                {
                    InvariantLowerCaseStringExtensions.ToLowerInvariant(buffer, 0, original.Length);

                    new string(buffer).Should().Be("the european hare (lepus europaeus)");
                }
            }
        }

        [Fact]
        public void BufferToLowerCase()
        {
            string original = "The European hare (Lepus europaeus)";
            var buffer = original.ToCharArray();

            InvariantLowerCaseStringExtensions.ToLowerInvariant(buffer, 0, original.Length);

            new string(buffer).Should().Be("the european hare (lepus europaeus)");
        }
    }
}