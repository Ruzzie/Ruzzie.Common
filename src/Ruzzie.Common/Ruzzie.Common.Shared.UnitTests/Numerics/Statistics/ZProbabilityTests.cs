using NUnit.Framework;
using Ruzzie.Common.Numerics.Distributions;

namespace Ruzzie.Common.Shared.UnitTests.Numerics.Statistics
{
    [TestFixture]
    public class ZProbabilityTests
    {
        [TestCase(0.0, 0.5)]
        [TestCase(1.0, 0.84134474616376287d)]
        public void ProbabilityOfZTests(double normalZValue, double expected)
        {
            Assert.That(ZProbability.ProbabilityOfZ(normalZValue), Is.EqualTo(expected));
        }
    }
}
