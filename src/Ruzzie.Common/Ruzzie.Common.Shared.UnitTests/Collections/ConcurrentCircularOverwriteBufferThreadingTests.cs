using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Ruzzie.Common.Collections;

namespace Ruzzie.Common.Shared.UnitTests.Collections
{
    [TestFixture]
    public class ConcurrentCircularOverwriteBufferThreadingTests
    {
        private static void ReadFromBufferWhileMustRead(ref bool mustRead, ConcurrentCircularOverwriteBuffer<int> buffer)
        {
            Stopwatch timer = new Stopwatch();
            var readCounter = 0;
            timer.Start();
            while (mustRead)
            {
                int value;
                if (buffer.ReadNext(out value))
                {
                }
                readCounter++;
            }
            timer.Stop();

            string message = "Total read calls:        " + readCounter;
            message += "\nAvg timer per read call: " + timer.Elapsed.TotalMilliseconds/readCounter + " ms.";
            message += "\nAvg timer per read call: " + (double) (timer.Elapsed.Ticks*100)/readCounter + " ns.";
            Trace.WriteLine(message);
        }

        private static void WriteToBufferWhileMustWrite(ref bool mustWrite, ConcurrentCircularOverwriteBuffer<int> buffer)
        {
            Stopwatch timer = new Stopwatch();
            var writeCounter = 0;
            timer.Start();
            while (mustWrite)
            {
                buffer.WriteNext(32);
                writeCounter++;
            }
            timer.Stop();

            string message = "Total write calls:        " + writeCounter;
            message += "\nAvg timer per write call: " + timer.Elapsed.TotalMilliseconds/writeCounter + " ms.";
            message += "\nAvg timer per write call: " + (double) (timer.Elapsed.Ticks*100)/writeCounter + " ns.";
            Trace.WriteLine(message);
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void ThreadHammerWritePerformanceTest()
        {
            //Arrange
            var cacheSize = 2048;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            var mustWrite = true;

            //Continuously write to the buffer
            Task writeLoop = Task.Run(() => { WriteToBufferWhileMustWrite(ref mustWrite, buffer); });

            Task writeLoopTwo = Task.Run(() => { WriteToBufferWhileMustWrite(ref mustWrite, buffer); });

            var mustRead = true;
            Task readLoop = Task.Run(() => { ReadFromBufferWhileMustRead(ref mustRead, buffer); });

            Thread.Sleep(1000);

            mustWrite = false;
            writeLoop.Wait();
            writeLoopTwo.Wait();
            mustRead = false;
            readLoop.Wait();
            //Assert: No Exceptions
        }

        [Test]
        public void ThreadSafeReadTests()
        {
            //Arrange
            var cacheSize = 8192;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i => { buffer.WriteNext(i); });

            ConcurrentDictionary<int, byte> allValuesHashSet = new ConcurrentDictionary<int, byte>();

            for (var i = 0; i < cacheSize; i++)
            {
                allValuesHashSet[i] = 1;
            }

            ConcurrentDictionary<int, byte> readValuesHashSet = new ConcurrentDictionary<int, byte>();

            //Act & Assert
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i =>
            {
                int readNext = buffer.ReadNext();
                readValuesHashSet.TryAdd(readNext, 1);
                Assert.That(allValuesHashSet.ContainsKey(readNext), Is.True, "Did not contain: " + readNext);
            });
            Assert.That(readValuesHashSet.Keys.Distinct().Count(), Is.EqualTo(cacheSize));
            Assert.That(buffer.Count, Is.EqualTo(0));
        }

        [Test]
        public void ThreadSafeWriteTests()
        {
            //Arrange
            var cacheSize = 2048;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            //Act
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i => { buffer.WriteNext(i); });

            //Assert
            int[] allValues = new int[cacheSize];
            buffer.CopyTo(allValues, 0);

            HashSet<int> allValuesHashSet = new HashSet<int>(allValues);

            Assert.That(buffer.Count, Is.EqualTo(cacheSize));
            for (var i = 0; i < cacheSize; i++)
            {
                Assert.That(allValuesHashSet.Contains(i), Is.True, "Did not contain: " + i);
            }
        }
    }
}