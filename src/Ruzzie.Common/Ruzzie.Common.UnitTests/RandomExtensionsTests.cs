using System;
using FluentAssertions;
using Xunit;

namespace Ruzzie.Common.UnitTests
{
    public class RandomExtensionsTests
    {
        [Fact]
        public void NextBytesThrowsExceptionWhenRandomIsNull()
        {
            Action act = () => RandomExtensions.NextBytes(null, 1);
            act.Should().Throw<Exception>();            
        }

        [Fact]
        public void NextBytesThrowsExceptionWhenCountIsLessThanOne()
        {
            Action act = () => new SimpleRandom().NextBytes(0);
            act.Should().Throw<Exception>();            
        }
    }
}
