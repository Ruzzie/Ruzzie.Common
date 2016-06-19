using System;
using Ruzzie.Common.Collections.Tests;
using Ruzzie.Common.Shared.UnitTests;

namespace Ruzzie.Common.Tests.Console
{
    class Program
    {
        static void Main(string[] args)
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

                System.Console.WriteLine("\t Current["+i+"]:> Average call time in ticks: " + callTimeInTicksPerTest);
            }
            
            System.Console.WriteLine("Result total:> Average call time in ticks: " + averageCallTimeInTicksPerTest / numberOfIterations);
          
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
