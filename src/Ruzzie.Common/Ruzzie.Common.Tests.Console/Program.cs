using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Liv.CommandlineArguments;
using Ruzzie.Common.Collections.Tests;
using Ruzzie.Common.Shared.UnitTests;

namespace Ruzzie.Common.Tests.Console
{
    class Program
    {
        [OptionsClass]
        public class OptionsDefinition : BaseOptionsClass
        {
            [Option(DefaultValue = "random_bytes.txt", ShortName = "o", Description = "The filename of file to write random bytes to.")]
            public string OptionGenerateBytesFileName { get; set; }
            [Option(DefaultValue = "5000000", ShortName = "c", Description = "The number of random bytes to generate", Type = typeof(int))]
            public int OptionGenerateBytesCount { get; set; }            
            [Option(DefaultValue = "false", ShortName = "t", Description = "Run randomness perf tests.", Type = typeof(bool))]
            public bool OptionRunTests { get; set; }
        }

        public static List<int> Factor(int number)
        {
            var results = new List<int>();
            int max = (int) Math.Sqrt(number);
            for (int factor = 1; factor <= max; ++factor)
            {
                if (number%factor == 0)
                {
                    results.Add(factor);
                    if (factor != number / factor)
                    { // Don't add the square root twice!  Thanks Jon
                        results.Add(number / factor);
                    }
                }
            }
            return results;
        }

        private static Tuple<int,int> maxFactors = new Tuple<int, int>(1,1);

        static void Main(string[] args)
        {
            //CalculateFactors();
            var arguments = ConsoleOptions.Init<OptionsDefinition>(args, true);
            arguments.PrintArguments();
            int seed = 1;//Environment.TickCount;
            int newSeed = seed;//((seed ) * 2)-1;// seed; //(seed*seed) + seed << 8;
            System.Console.WriteLine("New Seed: "+newSeed);
            //SimpleRandom random = new SimpleRandom( newSeed, 765492925, 210221731/*, 560185266, 369693244*/);
      
            //SimpleRandom random = new SimpleRandom( newSeed, 1053880252, 445034975/*, 560185266, 369693244*/);
            //SimpleRandom random = new SimpleRandom( newSeed, 1314548984, 1239452788/*, 560185266, 369693244*/);
            SimpleRandom random = new SimpleRandom( newSeed, 1609387687, 837541626/*, 560185266, 369693244*/);

            if (!string.IsNullOrWhiteSpace(arguments.OptionGenerateBytesFileName))
            {
                if (File.Exists(arguments.OptionGenerateBytesFileName))
                {
                    File.Delete(arguments.OptionGenerateBytesFileName);
                }

                using (var fs = File.OpenWrite(arguments.OptionGenerateBytesFileName))
                {
                    for (int i = 0; i < arguments.OptionGenerateBytesCount; i++)
                    {
                        fs.WriteByte(random.NextByte());                      
                    }
                }
            }


            if (arguments.OptionRunTests)
            {
                //first test correctness
                ConcurrentCircularOverwriteBufferTests correctNessTests = new ConcurrentCircularOverwriteBufferTests();
                correctNessTests.CountShouldNotExceedCapacity();
                correctNessTests.ReadNextThrowsExceptionWhenEmpty();
                correctNessTests.SmokeTest();
                correctNessTests.WriteNextShouldOverwriteValues();

                RandomPerformanceTests tests = new RandomPerformanceTests();
                tests.CompareSpeedForDifferentRandoms();

                System.Console.WriteLine("\n-- Different tests on custom random --\n");

                System.Console.WriteLine("\nMulti threaded performance test: Ruzzie.SimpleRandom with different thread count");

                var numberOfIterationsPerThread = 10000;

                decimal averageCallTimeInTicksPerTest = 0m;
                var numberOfIterations = 0;
                Random customRandom = new SimpleRandom(3);

                for (int i = 0; i < 100; i++)
                {

                    decimal callTimeInTicksPerTest = RunTestIteration(tests, customRandom, numberOfIterationsPerThread);
                    averageCallTimeInTicksPerTest += callTimeInTicksPerTest;
                    numberOfIterations++;

                    System.Console.WriteLine("\t Current[" + i + "]:> Average call time in ticks: " + callTimeInTicksPerTest);
                }

                System.Console.WriteLine("Result total:> Average call time in ticks: " + averageCallTimeInTicksPerTest/numberOfIterations);
            }

        }

        private static void CalculateFactors()
        {
            bool running = true;
            ConcurrentDictionary<int, List<int>> factorizations = new ConcurrentDictionary<int, List<int>>();
            Task calcFactorsTask = Task.Run(() =>
            {
                var part = Partitioner.Create(1396755359, 2147418000, 5000);
                Parallel.ForEach(part, i =>
                {
                    for (int fc = i.Item2; fc >= i.Item1; --fc)
                    {
                        var factors = Factor(fc);

                        var localMaxFactors = Volatile.Read(ref maxFactors);
                        if (factors.Count > localMaxFactors.Item2)
                        {
                            var newMaxFactors = new Tuple<int, int>(fc, factors.Count);
                            Interlocked.CompareExchange(ref maxFactors, newMaxFactors, localMaxFactors);
                            factorizations[fc] = factors;
                        }
                    }
                });

                running = false;
            });
            Task showFactorsStatus = Task.Run(() =>
            {
                while (running)
                {
                    Thread.Sleep(4000);
                    System.Console.WriteLine("Number: " + maxFactors.Item1 + " FactorCount: " + maxFactors.Item2);
                }
            });

            calcFactorsTask.Wait();
            showFactorsStatus.Wait();
            var groupby = factorizations.GroupBy(pair => pair.Key, pair => pair.Value.Count).OrderByDescending(ints => ints.Count());
            var numberWithMostFactorizations = groupby.First();
            System.Console.WriteLine("Most factors: " + numberWithMostFactorizations.Key);

            numberWithMostFactorizations.ToList().OrderBy(i => i).ToList().ForEach(i => System.Console.Write(i + ","));

            System.Console.WriteLine("");
        }

        private static decimal RunTestIteration(RandomPerformanceTests tests, Random customRandom, int numberOfIterationsPerThread)
        {
            var totalCallTimeInTicks = 0m;
            var numberOfTests = 0;
            System.Console.Write("\n[.");
            for (int i = Environment.ProcessorCount; i <= 64; i++)
            {
                //Multiple multi threaded performance

                var customTimingResult = tests.RunPerformanceTests(customRandom, i, numberOfIterationsPerThread);
                totalCallTimeInTicks += ((decimal) customTimingResult.NextBytes.TotalElapsedTicks)/
                                        (customTimingResult.NextBytes.NumberOfIterationsPerThread*customTimingResult.NextBytes.NumberOfThreads);

                numberOfTests++;
                System.Console.Write(".");
                //tests.PrintPerformanceTestResult(customTimingResult.NextBytes);
            }
            System.Console.Write("]\n");

            decimal averageCallTimeInTicksPerTest = (totalCallTimeInTicks/numberOfTests);
            return averageCallTimeInTicksPerTest;
        }
    }
}
