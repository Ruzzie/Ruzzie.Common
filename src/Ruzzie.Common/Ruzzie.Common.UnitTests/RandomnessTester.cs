using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ruzzie.Common.Numerics.Statistics;

namespace Ruzzie.Common.UnitTests
{
    public static class RandomnessTester
    {
        public static RandomnessTestResult TestInt(Random simpleRandom, int maxValue = int.MaxValue, int numberOfSamples = 10000)
        {
            var sampleResult = RunSamples(simpleRandom, numberOfSamples, maxValue);
            var randomnessTestResult = new RandomnessTestResult { SampleResult = sampleResult };

            return randomnessTestResult;
        }

        public static RandomnessTestResult TestBytes(Random simpleRandom, int numberOfSamples = 10000)
        {
            var sampleResult = RunSamplesBytes(simpleRandom, numberOfSamples, out var samples);
            var randomnessTestResult = new RandomnessTestResult { SampleResult = sampleResult };
            randomnessTestResult.Samples = samples;
            return randomnessTestResult;
        }

        private static SampleResult RunSamples(Random random, int numberOfSamples, int maxValue = int.MaxValue)
        {
            var histogram = new SortedDictionary<int, int>();

            double average = 0;
            for (int i = 0; i < numberOfSamples; i++)
            {
                int currentNumber = random.Next(maxValue);
                average = Average.StreamAverage(average, currentNumber, i);

                if (!histogram.ContainsKey(currentNumber))
                {
                    histogram[currentNumber] = 1;
                }
                else
                {
                    histogram[currentNumber] += 1;
                }
            }

            double chisq = Common.Numerics.Distributions.ChiSquared.ChiSquaredP(maxValue, numberOfSamples, histogram);
            double pochi = Common.Numerics.Distributions.ChiSquared.ProbabilityOfChiSquared(chisq, maxValue); // closer to 0.4 - 0.5 is better, <= 0.1 && >= 0.9 is bad
            double entropy = histogram.CalculateEntropy(numberOfSamples);//must be above 3, higher is better

            return new SampleResult(average, chisq, pochi, entropy);
        }

        private static SampleResult RunSamplesBytes(Random random, int numberOfSamples, out byte[] samples)
        {
            Dictionary<int, int> histogram = new Dictionary<int, int>(255);

            samples = new byte[numberOfSamples];
            double average = 0;
            for (int i = 0; i < numberOfSamples; i++)
            {
                byte currentNumber = random.NextBytes(1)[0];
                samples[i] = currentNumber;
                average = Average.StreamAverage(average, currentNumber, i);

                if (!histogram.ContainsKey(currentNumber))
                {
                    histogram[currentNumber] = 1;
                }
                else
                {
                    histogram[currentNumber] += 1;
                }
            }

            double chisq = 0;
            double pochi = 0;
            Task calculateChis = Task.Run(() =>
            {
                chisq = Common.Numerics.Distributions.ChiSquared.ChiSquaredP(byte.MaxValue + 1, numberOfSamples, histogram);
                pochi = Common.Numerics.Distributions.ChiSquared.ProbabilityOfChiSquared(chisq, byte.MaxValue + 1);// closer to 0.4 - 0.5 is better, <= 0.1 && >= 0.9 is bad

            });

            Task<double> calculateEntropy = Task.Run(() => histogram.CalculateEntropy(numberOfSamples) /*must be above 3, higher is better*/);

            calculateChis.Wait();
            return new SampleResult(average, chisq, pochi, calculateEntropy.Result);
        }
    }


    public class RandomnessTestResult
    {
        public SampleResult SampleResult { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte[] Samples { get; set; }
    }

    public struct SampleResult
    {
        private readonly double _average;
        private readonly double _chi;
        private readonly double _poChi;
        private readonly double _entropy;


        public SampleResult(double average, double chi, double poChi, double entropy)
        {
            _average = average;
            _chi = chi;
            _poChi = poChi;
            _entropy = entropy;
        }

        public double Average
        {
            get { return _average; }
        }

        public double Chi
        {
            get { return _chi; }
        }

        public double PoChi
        {
            get { return _poChi; }
        }

        public double Entropy
        {
            get { return _entropy; }
        }
    }
}