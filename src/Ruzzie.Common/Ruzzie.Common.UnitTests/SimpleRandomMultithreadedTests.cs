using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ruzzie.Common.UnitTests
{
    public class SimpleRandomMultiThreadedTests
    {
        [Fact/*(Skip = "These test cause timeouts on the buildserver....")*/]
        public void SmokeTestWithParallelFor()
        {
            var random = new SimpleRandom();

            Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = -1 }, i =>
            {
                var k = random.Next(i + 1);

                Parallel.For(0, 3000, (j) =>
                {
                    // ReSharper disable once UnusedVariable
                    var l = random.Next(j + 1) + k;
                });
            });

        }

        [Fact/*(Skip = "These test cause timeouts on the buildserver....")*/]
        public void SmokeTestWithParallelWhile()
        {// ReSharper disable AccessToModifiedClosure
            var random = new SimpleRandom();

            bool runLoop = true;
            Task whileTaskOne =
                Task.Run(() =>

                         {

                             while (runLoop)

                             {
                                 // ReSharper disable once UnusedVariable
                                 var k = random.Next(100);
                             }
                         }
                        );

            Task whileTaskTwo =
                Task.Run(() =>

                         {
                             while (runLoop)
                             {
                                 // ReSharper disable once UnusedVariable
                                 var k = random.Next(100);
                             }
                         }
                        );
            // ReSharper restore AccessToModifiedClosure
            Thread.Sleep(500);
            runLoop = false;
            whileTaskOne.Wait();
            whileTaskTwo.Wait();
        }
    }
}
