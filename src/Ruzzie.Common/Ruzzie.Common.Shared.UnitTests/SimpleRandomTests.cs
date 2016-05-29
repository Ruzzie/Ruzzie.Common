using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ruzzie.Common.Numerics.Statistics;

namespace Ruzzie.Common.Shared.UnitTests
{
    [TestFixture]
    public class SimpleRandomTests
    {

        [Test]
        [TestCase(0,10)]
        [TestCase(1,10)]
        [TestCase(-5,0)]
        [TestCase(-5,1)]       
        public void NextIntMinMax(int minValue, int maxValue)
        {
            SimpleRandom random = new SimpleRandom();

            for (int i = 0; i < 100; i++)
            {
                int result = random.Next(minValue, maxValue);

                Assert.That(result, Is.GreaterThanOrEqualTo(minValue));
                Assert.That(result, Is.LessThan(maxValue));
            }          
        }

        [Test]
        public void NextIntMinMaxReturnsMinValueWhenMinValueIsEqualToMaxValue()
        {
            Assert.That(new SimpleRandom().Next(1,1),Is.EqualTo(1));
        }

        [Test]
        public void NextIntMinMaxThrowsArgumentOutOfRangeExceptionWhenMaxValueIsLessThanMinValue()
        {

            Assert.That(()=>
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                new SimpleRandom().Next(10, 9);
            },Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            
        }
        [TestCase(10)]
        [TestCase(2)]
        [TestCase(99)]
        [TestCase(1)]
        public void NextIntMax(int maxValue)
        {
            SimpleRandom random = new SimpleRandom();

            for (int i = 0; i < 100; i++)
            {
                int result = random.Next(maxValue);

                Assert.That(result, Is.GreaterThanOrEqualTo(0));
                Assert.That(result, Is.LessThan(maxValue));
            }
        }

        [Test]
        public void NextIntMaxShouldReturnZeroWhenMaxValueIsZero()
        {
          Assert.That(new SimpleRandom().Next(0),Is.EqualTo(0));
        }


        [Test]
        public void NextIntMaxThrowsArgumentOutOfRangeExceptionWhenMaxValueIsLessThanZero()
        {

            Assert.That(() =>
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                new SimpleRandom().Next(-9);
            }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());

        }

        [Test]
        public void NextShouldNotReturnMax()
        {
            int initialSeed = 1;
            SimpleRandom random = new SimpleRandom(initialSeed);
            Assert.That(random.Next(10), Is.LessThan(10));
        }

        [Test]
        public void NextDoubleShouldBeLessThanOne()
        {
            SimpleRandom random = new SimpleRandom(1, 1664637461, 476397391);
            int sampleSize = 127500;

            List<double> samples = new List<double>(sampleSize);
            double average = 0;

            for (int i = 0; i < sampleSize; i++)
            {
                double currentNumber = random.NextDouble();

                average = Average.StreamAverage(average, currentNumber, i);
                samples.Add(currentNumber);
            }

            Console.WriteLine("Min: " + samples.Min());
            Console.WriteLine("Max: " + samples.Max());
            Assert.That(samples.Contains(1.0), Is.False);
            Assert.That(average, Is.EqualTo(0.49953671863098814d));
        }

        [Test]
        public void TestValues()
        {
            SimpleRandom simpleRandom = new SimpleRandom(1, 862314265, 311308189);

            Assert.That(simpleRandom.NextByte(), Is.EqualTo(20));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(5));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(92));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(4));
        }

        [Test]
        public void NextByteSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.NextByte(), Is.EqualTo(247));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(43));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(85));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(57));
        }

        [Test]
        public void NextIntSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.Next(), Is.EqualTo(886818807));
            Assert.That(simpleRandom.Next(), Is.EqualTo(737928235));
            Assert.That(simpleRandom.Next(), Is.EqualTo(929410901));
            Assert.That(simpleRandom.Next(), Is.EqualTo(322689593));
        }

        [Test]
        public void NextInt100SmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.Next(100), Is.EqualTo(54));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(82));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(1));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(40));
        }

        [Test]
        public void NextIntAverageTest()
        {
           
            SimpleRandom random = new SimpleRandom();
            int sampleSize = 500000;

            List<int> samples = new List<int>(sampleSize);

            for (int i = 0; i < sampleSize; i++)
            {
                samples.Add(random.Next(100));
            }

            Console.WriteLine("Min: " + samples.Min());
            Console.WriteLine("Max: " + samples.Max());
            Assert.That(samples.Contains(99), Is.True);
            Assert.That(samples.Contains(0), Is.True);
            Assert.That(samples.Select(b => b).Average(), Is.EqualTo(49.517831999999999d));

        }

        [Test]
        public void RandomnessTesterTest()
        {
            int maxValue = 100;
            RandomnessTestResult result = RandomnessTester.TestInt(new SimpleRandom(), maxValue);

            Assert.That(result.SampleResult.Average, Is.EqualTo(49.558000000000177d));
            Assert.That(result.SampleResult.Chi, Is.EqualTo(101.92d).Within(88.099999999999994 - 88.09999999999998));
            Assert.That(result.SampleResult.PoChi, Is.EqualTo(0.42779397775959888d).Within(0.79665006850430176 - 0.79665006850429465d));
            Assert.That(result.SampleResult.Entropy, Is.EqualTo(6.6365443165163915d));
        }

        [Test]
        public void RandomnessBytesTesterTest()
        {
            RandomnessTestResult result = RandomnessTester.TestBytes(new SimpleRandom(37));

            Assert.That(result.SampleResult.Average, Is.EqualTo(128.8636000000005d));
            Assert.That(result.SampleResult.Chi, Is.EqualTo(276.91519999999997d).Within(224.86950000000009 - 224.86949999999999d)); //88.09999999999998d
            Assert.That(result.SampleResult.PoChi, Is.EqualTo(0.17629799433916726d).Within(0.0000000000001));//0.79665006850430176d
            Assert.That(result.SampleResult.Entropy, Is.EqualTo(7.980085736317049d));
        }

        [Test]
        public void ResetWithSameSeedShouldReturnSameSequence()
        {
            SimpleRandom random = new SimpleRandom();

            int value = random.Next();

            random.Reset(1);

            Assert.That(value, Is.EqualTo(random.Next()).And.Not.EqualTo(random.Next()));

        }
    }

}
