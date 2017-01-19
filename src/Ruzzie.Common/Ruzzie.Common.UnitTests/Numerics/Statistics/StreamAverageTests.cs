using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Ruzzie.Common.UnitTests.Numerics.Statistics
{
    [TestFixture]
    public class StreamAverageTests
    {
        [TestCase(new double[] { 1, 1, 1 }, 1.0)]
        [TestCase(new double[] { 1, 1, 1, 2, 2, 2 }, 1.5)]
        [TestCase(new double[] { 1, 2 }, 1.5)]
        [TestCase(new double[] { 0, 9 }, 4.5)]
        [TestCase(new double[] { 0, 10, 0, 10, 0, 10 }, 5)]
        public void DoubleTests(double[] items, double expected)
        {
            double avg = 0;
            for (int i = 0; i < items.Length; i++)
            {
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, items[i], i);
            }

            Assert.That(avg, Is.EqualTo(expected));
            Assert.That(avg, Is.EqualTo(items.Average()));
        }

        [TestCase(new[] { 1, 1, 1 }, 1.0)]
        [TestCase(new[] { 1, 1, 1, 2, 2, 2 }, 1.5)]
        [TestCase(new[] { 1, 2 }, 1.5)]
        [TestCase(new[] { 0, 9 }, 4.5)]
        [TestCase(new[] { 0, 10, 0, 10, 0, 10 }, 5)]
        [TestCase(new[] { 1, 2, 3, 1, 2, 4 }, 2.1666666666666665)]
        public void StreamAverageIntTests(int[] items, double expected)
        {
            double avg = 0;
            for (int i = 0; i < items.Length; i++)
            {
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, items[i], i);
            }

            Assert.That(avg, Is.EqualTo(expected));
            Assert.That(avg, Is.EqualTo(items.Average()));
        }

        [Test]
        public void StreamAverageForSetTest()
        {
            Random random = new Random(1);
            double avg = 0;
            List<int> samples = new List<int>(1000);
            for (int i = 0; i < 1000; i++)
            {
                int value = random.Next();
                avg = Common.Numerics.Statistics.Average.StreamAverage(avg, value, i);
                samples.Add(value);
            }

            Assert.That(avg, Is.EqualTo(1081420669.4020009d));
            Assert.That(avg, Is.EqualTo(samples.Average()).Within(0.000009));
        }
    }
}