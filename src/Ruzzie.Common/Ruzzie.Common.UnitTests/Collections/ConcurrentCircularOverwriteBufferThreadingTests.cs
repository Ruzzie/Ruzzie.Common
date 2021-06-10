using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Collections
{
    public class ConcurrentCircularOverwriteBufferThreadingTests
    {
        public ITestOutputHelper Output { get; }

        public ConcurrentCircularOverwriteBufferThreadingTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private static void ReadFromBufferWhileMustRead(ref bool mustRead, ConcurrentCircularOverwriteBuffer<int> buffer, ITestOutputHelper output)
        {
            Stopwatch timer = new Stopwatch();
            var readCounter = 0;
            timer.Start();
            while (mustRead)
            {
                if (buffer.ReadNext(out _))
                {
                }
                ++readCounter;
            }
            timer.Stop();

            string message = "Total read calls:        " + readCounter;
            message += "\nAvg timer per read call: " + timer.Elapsed.TotalMilliseconds/readCounter + " ms.";
            message += "\nAvg timer per read call: " + (double) (timer.Elapsed.Ticks*100)/readCounter + " ns.";
            output.WriteLine(message);
        }

        private static void WriteToBufferWhileMustWrite(ref bool mustWrite, ConcurrentCircularOverwriteBuffer<int> buffer, ITestOutputHelper output)
        {
            Stopwatch timer = new Stopwatch();
            var writeCounter = 0;
            timer.Start();
            while (mustWrite)
            {
                buffer.WriteNext(32);
                ++writeCounter;
            }
            timer.Stop();

            string message = "Total write calls:        " + writeCounter;
            message += "\nAvg timer per write call: " + timer.Elapsed.TotalMilliseconds/writeCounter + " ms.";
            message += "\nAvg timer per write call: " + (double) (timer.Elapsed.Ticks*100)/writeCounter + " ns.";
            output.WriteLine(message);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void ThreadHammerWritePerformanceTest()
        {
            //Arrange
            var cacheSize = 2048;
            var buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            var mustWrite = true;

            //Continuously write to the buffer
            Task writeLoop = Task.Run(() => { WriteToBufferWhileMustWrite(ref mustWrite, buffer, Output); });

            Task writeLoopTwo = Task.Run(() => { WriteToBufferWhileMustWrite(ref mustWrite, buffer, Output); });

            var mustRead = true;
            Task readLoop = Task.Run(() => { ReadFromBufferWhileMustRead(ref mustRead, buffer, Output); });

            Thread.Sleep(1000);

            mustWrite = false;
            writeLoop.Wait();
            writeLoopTwo.Wait();
            mustRead = false;
            readLoop.Wait();
            //Assert: No Exceptions
        }

        [Fact]
        public void ThreadSafeReadTests()
        {
            //Arrange
            var cacheSize = 8192;
            var buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i => { buffer.WriteNext(i); });

            var allValuesHashSet = new ConcurrentDictionary<int, byte>();

            for (var i = 0; i < cacheSize; i++)
            {
                allValuesHashSet[i] = 1;
            }

            var readValuesHashSet = new ConcurrentDictionary<int, byte>();

            //Act & Assert
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i =>
            {
                int readNext = buffer.ReadNext();
                readValuesHashSet.TryAdd(readNext, 1);

                allValuesHashSet.ContainsKey(readNext).Should().BeTrue("Did not contain: " + readNext);
            });

            readValuesHashSet.Keys.Distinct().Count().Should().Be(cacheSize);
            buffer.Count.Should().Be(0);
        }

        [Fact]
        public void ThreadSafeWriteTests()
        {
            //Arrange
            var cacheSize = 2048;
            var buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            //Act
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1}, i => { buffer.WriteNext(i); });

            //Assert
            int[] allValues = new int[cacheSize];
            buffer.CopyTo(allValues, 0);

            var allValuesHashSet = new HashSet<int>(allValues);

            buffer.Count.Should().Be(cacheSize);

            for (var i = 0; i < cacheSize; i++)
            {
                allValuesHashSet.Should().Contain(i, "Did not contain: " + i);
            }
        }

        [Fact(Skip = "disables, this reproduces multi-reader, multi-writer concurrency problem")]
        public void ThreadRaceConditionWithNullTests()
        {
            //Arrange
            var cacheSize = 1280;
            var buffer = new ConcurrentCircularOverwriteBuffer<ValueContainer>(cacheSize);

            //Act
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = 4},
                i =>
                {
                    if (!buffer.ReadNext(out var readValue))
                    {
                        buffer.WriteNext(new ValueContainer{Data = new List<int>{i}});
                    }
                    else
                    {
                        readValue.Should().NotBeNull();
                        readValue.Data.Should().NotBeNull();
                        readValue.Data = null;
                    }

                    if (!buffer.ReadNext(out var readValueTwo))
                    {
                        buffer.WriteNext(new ValueContainer{Data = new List<int>{i}});
                    }
                    else
                    {
                        readValueTwo.Should().NotBeNull();
                        readValueTwo.Data.Should().NotBeNull();
                        readValueTwo.Data = null;
                    }

                    if (buffer.ReadNext(out var readValueThree))
                    {
                        readValueThree.Should().NotBeNull();
                        readValueThree.Data.Should().NotBeNull();
                        readValueThree.Data = null;
                    }
                });

            //Assert
            buffer.Count.Should().Be(0);
        }

        class ValueContainer
        {
            public List<int> Data { get; set; }
        }
    }
}