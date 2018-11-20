using FluentAssertions;
using Ruzzie.Common.Numerics.Distributions;
using Xunit;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics
{
    public class ZProbabilityTests
    {
#if !NET40
        [Theory]
        [InlineData(0.0, 0.5)]
        [InlineData(1.0, 0.84134474616376287d)]
        public void ProbabilityOfZTests(double normalZValue, double expected)
        {
            ZProbability.ProbabilityOfZ(normalZValue).Should().Be(expected);
        }
#endif
    }
}
