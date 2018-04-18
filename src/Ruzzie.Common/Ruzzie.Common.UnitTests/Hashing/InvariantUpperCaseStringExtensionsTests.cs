using System;
using FluentAssertions;
using Ruzzie.Common.Hashing;
using Xunit;

namespace Ruzzie.Common.UnitTests.Hashing
{    
    public class InvariantUpperCaseStringExtensionsTests
    {
        [Theory]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]        
        [InlineData("", "")]
        [InlineData("3 Harvard Square", "3 HARVARD SQUARE")]
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
    }
}