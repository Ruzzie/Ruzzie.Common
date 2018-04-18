using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Ruzzie.Common.UnitTests
{    
    public class RandomPerformanceTests
    {
        [Fact(Skip = "Intëgration test to test algorithm. Takes too long on CI")]        
        public void CompareSpeedForDifferentRandoms()
        {
            Random systemRandom = new Random(3);
            Random customRandom = new SimpleRandom(3);

            //Single threaded performance
            var singleThreadIterations = 10000 * 2052;
            var systemTimingResult = RunPerformanceTests(systemRandom, 1, singleThreadIterations);
            var customTimingResult = RunPerformanceTests(customRandom, 1, singleThreadIterations);

            Console.WriteLine("Single threaded performance test: System.Random");
            Console.WriteLine("Bytes: ");
            PrintPerformanceTestResult(systemTimingResult.NextBytes);

            Console.WriteLine("\nSingle threaded performance test: Ruzzie.SimpleRandom");
            Console.WriteLine("Bytes: ");
            PrintPerformanceTestResult(customTimingResult.NextBytes);

            //Multi threaded performance

            systemTimingResult = RunPerformanceTests(systemRandom, 64, 100000);
            customTimingResult = RunPerformanceTests(customRandom, 64, 100000);

            Console.WriteLine("\nMulti threaded performance test: System.Random");
            Console.WriteLine("Bytes: ");
            PrintPerformanceTestResult(systemTimingResult.NextBytes);

            Console.WriteLine("\nMulti threaded performance test: Ruzzie.SimpleRandom");
            Console.WriteLine("Bytes: ");
            PrintPerformanceTestResult(customTimingResult.NextBytes);
        }

        public void PrintPerformanceTestResult(PerformanceTestResult result)
        {
            Console.WriteLine("\t "+nameof(result.NumberOfThreads)+": "+ result.NumberOfThreads);
            Console.WriteLine("\t "+nameof(result.NumberOfIterationsPerThread)+": "+ result.NumberOfIterationsPerThread);
            Console.WriteLine("\t " + nameof(result.TotalElapsedTicks) + ": " + result.TotalElapsedTicks);
            Console.WriteLine("\t "+nameof(result.TotalElapsedTimeInMilliseconds)+": "+ result.TotalElapsedTimeInMilliseconds);
        }

        public RandomPerformanceTestTimingResults RunPerformanceTests(Random random, int numberOfThreads = 1, int numberOfIterationsPerThread = 10000)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            if (numberOfThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));
            }
            if (numberOfIterationsPerThread <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfIterationsPerThread));
            }

            RandomPerformanceTestTimingResults results = new RandomPerformanceTestTimingResults();
            PerformanceTestResult bytesResult = TestNextBytes(random, numberOfThreads, numberOfIterationsPerThread);

            results.NextBytes = bytesResult;

            return results;
        }

        private PerformanceTestResult TestNextBytes(Random random, int numberOfThreads, int numberOfIterationsPerThread)
        {
            PerformanceTestResult result = new PerformanceTestResult();
            Stopwatch sw = new Stopwatch();

            ParallelOptions options = new ParallelOptions() {MaxDegreeOfParallelism = numberOfThreads};
            sw.Start();
            Parallel.For(0, numberOfThreads, options, counter =>
            {
                for (int i = 0; i < numberOfIterationsPerThread; i++)
                {
                    random.NextBytes(1);
                }
            });
            sw.Stop();

            result.NumberOfThreads = numberOfThreads;
            result.NumberOfIterationsPerThread = numberOfIterationsPerThread;
            result.TotalElapsedTimeInMilliseconds = sw.ElapsedMilliseconds;
            result.TotalElapsedTicks = sw.ElapsedTicks;
            return result;
        }
    }

    public class PerformanceTestResult
    {
        public int NumberOfThreads { get; set; }
        public int NumberOfIterationsPerThread { get; set; }
        public long TotalElapsedTimeInMilliseconds { get; set; }
        public long TotalElapsedTicks { get; set; }
    }

    public class RandomPerformanceTestTimingResults
    {
        public PerformanceTestResult NextBytes { get; set; }
    }
}
