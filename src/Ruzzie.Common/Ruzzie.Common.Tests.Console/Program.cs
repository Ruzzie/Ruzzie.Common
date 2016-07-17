using System;
using System.IO;
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
            public bool OptionRunTests { get; set; }//5000000
        }

        static void Main(string[] args)
        {
            var arguments = ConsoleOptions.Init<OptionsDefinition>(args, true);
            arguments.PrintArguments();
            int seed = Environment.TickCount;
            int newSeed = 1;// seed; //(seed*seed) + seed << 8;
            System.Console.WriteLine("New Seed: "+newSeed);
            SimpleRandom random = new SimpleRandom( newSeed/*, 560185266, 369693244*/);

            if (!string.IsNullOrWhiteSpace(arguments.OptionGenerateBytesFileName))
            {
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
