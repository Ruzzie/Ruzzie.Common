using FluentAssertions;
using Ruzzie.Common;
using Xunit;

namespace Ruzzie.Common.UnitTests
{
    public class StripTests
    {
        [InlineData("Doctor Who!",       "Doctor Who")]
        [InlineData(" Flashback {4}{R}", " Flashback 4R")]
        [InlineData(" Æther Vial",       " ther Vial")]
        [InlineData(" Æther Vial-",      " ther Vial-")]
        [Theory]
        public void SmokeTest(string input, string expected)
        {
            StringExtensions.StripAlternative(input).Should().Be(expected);
        }
    }
}