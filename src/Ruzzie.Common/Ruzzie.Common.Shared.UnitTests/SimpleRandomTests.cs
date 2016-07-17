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
        public void NextBytesThrowsExceptionWhenBufferIsNull()
        {
            SimpleRandom random = new SimpleRandom();

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(()=> random.NextBytes(null), Throws.Exception);
        }

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
            Assert.That(average, Is.EqualTo(0.50025237100076547d));
        }

        [Test]
        public void TestValues()
        {
            SimpleRandom simpleRandom = new SimpleRandom(1, 862314265, 311308189);

            Assert.That(simpleRandom.NextByte(), Is.EqualTo(242));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(248));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(173));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(79));
        }

        [Test]
        public void NextByteSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.NextByte(), Is.EqualTo(244));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(50));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(33));
            Assert.That(simpleRandom.NextByte(), Is.EqualTo(13));
        }

        [Test]
        public void NextIntSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.Next(), Is.EqualTo(523522292));
            Assert.That(simpleRandom.Next(), Is.EqualTo(191336242));
            Assert.That(simpleRandom.Next(), Is.EqualTo(74751265));
            Assert.That(simpleRandom.Next(), Is.EqualTo(635807757));
        }

        [Test]
        public void NextInt100SmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            Assert.That(simpleRandom.Next(100), Is.EqualTo(92));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(42));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(65));
            Assert.That(simpleRandom.Next(100), Is.EqualTo(57));
        }

        [Test]
        public void NextIntAverageTest()
        {
           
            SimpleRandom random = new SimpleRandom(2332454);
            int sampleSize = 500000;

            List<int> samples = new List<int>(sampleSize);

            for (int i = 0; i < sampleSize; i++)
            {
                samples.Add(random.Next(100));
            }

            Console.WriteLine("Min: " + samples.Min());
            Console.WriteLine("Max: " + samples.Max());

            Assert.That(samples.Contains(0), Is.True, " Does not contain 0");
            Assert.That(samples.Contains(99), Is.True, " Does not contain 99");
            Assert.That(samples.Select(b => b).Average(), Is.EqualTo(49.500860000000003d));

        }

        [Test]
        public void RandomnessTesterTest()
        {
            int maxValue = 100;
            RandomnessTestResult result = RandomnessTester.TestInt(new SimpleRandom(), maxValue);

            Assert.That(result.SampleResult.Average, Is.EqualTo(49.263999999999811d));
            Assert.That(result.SampleResult.Chi, Is.EqualTo(80.160000000000011d).Within(0.01d));
            Assert.That(result.SampleResult.PoChi, Is.EqualTo(0.92787825698550153d).Within(0.00001d));
            Assert.That(result.SampleResult.Entropy, Is.EqualTo(6.6380897597798931d));
        }

        [Test]
        public void RandomnessBytesTesterTest()
        {
            RandomnessTestResult result = RandomnessTester.TestBytes(new SimpleRandom(37));

            Assert.That(result.SampleResult.Average, Is.EqualTo(126.5008d));
            Assert.That(result.SampleResult.Chi, Is.EqualTo(222.43839999999997d).Within(0.1d)); //88.09999999999998d
            Assert.That(result.SampleResult.PoChi, Is.EqualTo(0.93620808281449741d).Within(0.0000000000001));//0.79665006850430176d
            Assert.That(result.SampleResult.Entropy, Is.EqualTo(7.9838303706956477d));
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
