using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ruzzie.Common.Numerics.Statistics;
using Xunit;

namespace Ruzzie.Common.UnitTests
{
    public class SimpleRandomTests
    {
#if !NET40 
        [Fact]
        public void NextBytesThrowsExceptionWhenBufferIsNull()
        {
            SimpleRandom random = new SimpleRandom();
            // ReSharper disable once AssignNullToNotNullAttribute
            Action act = (() => random.NextBytes(null));
            
            act.Should().Throw<Exception>();
        }


        [Theory]
        [InlineData(0,10)]
        [InlineData(1,10)]
        [InlineData(-5,0)]
        [InlineData(-5,1)]       
        public void NextIntMinMax(int minValue, int maxValue)
        {
            SimpleRandom random = new SimpleRandom();

            for (int i = 0; i < 100; i++)
            {
                int result = random.Next(minValue, maxValue);
                
                result.Should().BeGreaterOrEqualTo(minValue);                
                result.Should().BeLessThan(maxValue);
            }          
        }
#endif
        [Fact]
        public void NextIntMinMaxReturnsMinValueWhenMinValueIsEqualToMaxValue()
        {            
            new SimpleRandom().Next(1, 1).Should().Be(1);
        }
#if !NET40
        [Fact]
        public void NextIntMinMaxThrowsArgumentOutOfRangeExceptionWhenMaxValueIsLessThanMinValue()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Action act = ()=> new SimpleRandom().Next(10, 9);
            act.Should().Throw<ArgumentOutOfRangeException>();                        
        }
        [Theory]
        [InlineData(10)]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(1)]
        public void NextIntMax(int maxValue)
        {
            SimpleRandom random = new SimpleRandom();

            for (int i = 0; i < 100; i++)
            {
                int result = random.Next(maxValue);
                
                result.Should().BeGreaterOrEqualTo(0);                
                result.Should().BeLessThan(maxValue);
            }
        }
#endif
        [Fact]
        public void NextIntMaxShouldReturnZeroWhenMaxValueIsZero()
        {
            new SimpleRandom().Next(0).Should().Be(0);
        }
#if !NET40
        [Fact]
        public void NextIntMaxThrowsArgumentOutOfRangeExceptionWhenMaxValueIsLessThanZero()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Action act = ()=> new SimpleRandom().Next(-9);
            
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
#endif
        [Fact]
        public void NextShouldNotReturnMax()
        {
            int initialSeed = 1;
            SimpleRandom random = new SimpleRandom(initialSeed);
            
            random.Next(10).Should().BeLessThan(10);
        }

        [Fact]
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
            
            samples.Should().NotContain(1.0);            
            average.Should().Be(0.50025237100076547d);
        }

        [Fact]
        public void TestValues()
        {
            SimpleRandom simpleRandom = new SimpleRandom(1, 862314265, 311308189);            

            simpleRandom.NextByte().Should().Be(242);
            simpleRandom.NextByte().Should().Be(248);
            simpleRandom.NextByte().Should().Be(173);
            simpleRandom.NextByte().Should().Be(79);
        }

        [Fact]
        public void NextByteSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            simpleRandom.NextByte().Should().Be(175);
            simpleRandom.NextByte().Should().Be(211);
            simpleRandom.NextByte().Should().Be(17);
            simpleRandom.NextByte().Should().Be(98);
        }

        [Fact]
        public void NextIntSmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom(); 

            simpleRandom.Next().Should().Be(999359663);
            simpleRandom.Next().Should().Be(1963915219);
            simpleRandom.Next().Should().Be(1719644689);
            simpleRandom.Next().Should().Be(1676061794);
        }

        [Fact]
        public void NextInt100SmokeTest()
        {
            SimpleRandom simpleRandom = new SimpleRandom();

            simpleRandom.Next(100).Should().Be(63);
            simpleRandom.Next(100).Should().Be(19);
            simpleRandom.Next(100).Should().Be(89);
            simpleRandom.Next(100).Should().Be(94);
        }

        [Fact]
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
            
            samples.Should().Contain(0);
            samples.Should().Contain(99);
            samples.Select(b => b).Average().Should().Be(49.518106000000003d);
        }

        [Fact]
        public void RandomnessTesterTest()
        {
            int maxValue = 100;
            RandomnessTestResult result = RandomnessTester.TestInt(new SimpleRandom(), maxValue);

            result.SampleResult.Average.Should().Be(49.862699999999855d);
            result.SampleResult.Chi.Should().BeApproximately(77.639999999999986d, 0.01d);
            result.SampleResult.PoChi.Should().BeApproximately(0.95244956125302926d, 0.00001d);
            result.SampleResult.Entropy.Should().BeApproximately(6.6382366673825759d, 0.00001d);
        }

#if !NET40        
        [Fact]
 #endif
        public void RandomnessBytesTesterTest()
        {
            RandomnessTestResult result = RandomnessTester.TestBytes(new SimpleRandom(37));

            result.SampleResult.Average.Should().Be(127.68689999999995d);
            result.SampleResult.Chi.Should().BeApproximately(242.0992d, 0.01d);
            result.SampleResult.PoChi.Should().BeApproximately(0.7244845588804244d, 0.0000000000001);
            result.SampleResult.Entropy.Should().Be(7.9824575806072193d);
        }

        [Fact]
        public void ResetWithSameSeedShouldReturnSameSequence()
        {
            SimpleRandom random = new SimpleRandom();

            int value = random.Next();

            random.Reset(1);
            
            value.Should().Be(random.Next()).And.NotBe(random.Next());
        }

        [Fact]
        public void RandomNextWithOnlyMaxShouldReturnSameAsNextWithMinAndMaxAfterReset()
        {
            //I don't know if this is a valid case

            SimpleRandom random = new SimpleRandom(1);
            random.Reset(123123);

            int value = random.Next(4);
            value.Should().Be(0);
           
            random.Reset(123123);

            value = random.Next(0, 4);
            value.Should().Be(2);
        }
    }
}
