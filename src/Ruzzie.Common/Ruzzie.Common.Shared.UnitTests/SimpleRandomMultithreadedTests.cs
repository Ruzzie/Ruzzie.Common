using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ruzzie.Common.Shared.UnitTests
{
    [TestFixture][Ignore("These test cause timeouts on the buildserver....")]
    public class SimpleRandomMultiThreadedTests
    {
        [Test][RequiresThread]
        public void SmokeTestWithParallelFor()
        {
            SimpleRandom random = new SimpleRandom();

            Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, i =>
            {
                var k = random.Next(i + 1);

                Parallel.For(0, 3000, (j) =>
                {
                    // ReSharper disable once UnusedVariable
                    var l = random.Next(j + 1) + k;
                });
            });

        }

        [Test][RequiresThread]
        public void SmokeTestWithParallelWhile()
        {// ReSharper disable AccessToModifiedClosure
            SimpleRandom random = new SimpleRandom();

            bool runLoop = true;
            Task whileTaskOne = Task.Run(() =>
            {

                while (runLoop)

                {
                    // ReSharper disable once UnusedVariable
                    var k = random.Next(100);
                }
            }
                );

            Task whileTaskTwo = Task.Run(() =>
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
