﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ruzzie.Common.Collections.Tests
{
    [TestFixture]
    public class ConcurrentCircularOverwriteBufferTests
    {
        [Test]
        public void SmokeTest()
        {
            int size = 2;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

            buffer.WriteNext(1);
            buffer.WriteNext(2);

            Assert.That(buffer.ReadNext(), Is.EqualTo(1));
            Assert.That(buffer.ReadNext(), Is.EqualTo(2));
        }

        [Test]
        public void CountShouldNotExceedCapacity()
        {
            int size = 2;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);

            Assert.That(buffer.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReadNextThrowsExceptionWhenEmpty()
        {
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

            Assert.That(() => buffer.ReadNext(), Throws.Exception);
        }

        [Test]
        public void WriteNextShouldOverwriteValues()
        {
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

            for (int i = 0; i < 10; i++)
            {
                buffer.WriteNext(i);
            }

            Assert.That(buffer.ReadNext(), Is.EqualTo(8));
            Assert.That(buffer.ReadNext(), Is.EqualTo(9));
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void ThreadHammerWritePerformanceTest()
        {
            //Arrange
            int cacheSize = 2048;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            bool mustWrite = true;

            //Continuously write to the buffer
            Task writeLoop = Task.Run(() =>
            {
                LoopWrite(ref mustWrite, buffer);
            });

            Task writeLoopTwo = Task.Run(() =>
            {
                LoopWrite(ref mustWrite, buffer);
            });

            bool mustRead = true;
            Task readLoop = Task.Run(() =>
            {
                LoopRead(ref mustRead, buffer);
            });

            Thread.Sleep(1000);

            mustWrite = false;
            writeLoop.Wait();
            writeLoopTwo.Wait();
            mustRead = false;
            readLoop.Wait();
            //Assert: No Exceptions
        }

        private static void LoopRead(ref bool mustRead, ConcurrentCircularOverwriteBuffer<int> buffer)
        {
            Stopwatch timer = new Stopwatch();
            int readCounter = 0;
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

        private static void LoopWrite(ref bool mustWrite, ConcurrentCircularOverwriteBuffer<int> buffer)
        {
            Stopwatch timer = new Stopwatch();
            int writeCounter = 0;
            timer.Start();
            while (mustWrite)
            {
                buffer.WriteNext(32);
                writeCounter++;
            }
            timer.Stop();

            string message = "Total write calls:        " + writeCounter;
            message += "\nAvg timer per write call: " + timer.Elapsed.TotalMilliseconds / writeCounter + " ms.";
            message += "\nAvg timer per write call: " + (double)(timer.Elapsed.Ticks * 100) / writeCounter + " ns.";
            Trace.WriteLine(message);
        }

        [Test]
        public void ThreadSafeWriteTests()
        {
            //Arrange
            int cacheSize = 2048;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            //Act
            Parallel.For(0, cacheSize, new ParallelOptions {MaxDegreeOfParallelism = -1},i =>
            {              
                buffer.WriteNext(i);                        
            });

            //Assert
            int[] allValues = new int[cacheSize];
            buffer.CopyTo(allValues,0);

            HashSet<int> allValuesHashSet = new HashSet<int>(allValues);

            Assert.That(buffer.Count, Is.EqualTo(cacheSize));
            for (int i = 0; i < cacheSize; i++)
            {
                Assert.That(allValuesHashSet.Contains(i), Is.True,  "Did not contain: " + i);
            }
        }

        [Test]
        public void ThreadSafeReadTests()
        {
            //Arrange
            int cacheSize = 8192;
            ConcurrentCircularOverwriteBuffer<int> buffer = new ConcurrentCircularOverwriteBuffer<int>(cacheSize);

            Parallel.For(0, cacheSize,new ParallelOptions {MaxDegreeOfParallelism = -1} , i =>
            {
                buffer.WriteNext(i);
            });

            ConcurrentDictionary<int, byte> allValuesHashSet = new ConcurrentDictionary<int, byte>();

            for (int i = 0; i < cacheSize; i++)
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
            Assert.That(readValuesHashSet.Keys.Distinct().Count() , Is.EqualTo(cacheSize));
            Assert.That(buffer.Count, Is.EqualTo(0));
        }

        [TestCase(1)]
        [TestCase(0)]
        [TestCase(-1)]
        public void ConstructorThrowsArgumentExceptionWhenSizeIsLessThanTwo(int size)
        {
            Assert.That(()=> new ConcurrentCircularOverwriteBuffer<int>(size), Throws.Exception);
        }
    }
}