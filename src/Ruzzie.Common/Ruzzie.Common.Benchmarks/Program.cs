﻿using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Ruzzie.Common.Collections;

namespace Ruzzie.Common.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var allBenchmarks = BenchmarkRunner.Run(new[] { typeof(QueueBufferReadWriteOne) });
        }
    }

    [GcServer(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    [AllStatisticsColumn]
    public class QueueBufferReadUnderPressure
    {
        private readonly QueueBuffer<int>    _queueBuffer;
        private readonly QueueBufferAlt<int> _altBuffer;

        private readonly int[] _queueOutputPlaceholder = new int[N];

        private const int N = 8388607 >> 7;

        private const    int    ConcurrentProducersCount = 2;
        private readonly Task[] _producerTasks           = new Task[ConcurrentProducersCount];
        private readonly Task[] _altProducerTasks        = new Task[ConcurrentProducersCount];

        public QueueBufferReadUnderPressure()
        {
            _queueBuffer = new QueueBuffer<int>(N);
            _altBuffer   = new QueueBufferAlt<int>(N);
            //new TaskFactory(TaskCreationOptions.LongRunning,TaskContinuationOptions.LongRunning)
        }

        private volatile bool _running;

        private void AddItems()
        {
            int trivial  = 0;
            var spinWait = new SpinWait();
            while (_running)
            {
                bool addResult = _queueBuffer.TryAdd(++trivial);
                if (addResult == false)
                    spinWait.SpinOnce();
                /*else
                    spinWait.Reset();*/
            }
        }

        private void AddItemsAlt()
        {
            int trivial = 0;
            //var spinWait = new SpinWait();
            while (_running)
            {
                bool addResult = _altBuffer.TryAdd(++trivial);
                /*if (addResult == false)
                    spinWait.SpinOnce();*/
                /*else
                    spinWait.Reset();*/
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(" // GLOBAL SETUP!");
            _running = true;
            for (int i = 0; i < ConcurrentProducersCount; i++)
            {
                _producerTasks[i]    = Task.Run(AddItems);
                _altProducerTasks[i] = Task.Run(AddItemsAlt);
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Console.WriteLine(" // GLOBAL CLEANUP!");
            _running = false;
            Task.WhenAll(_producerTasks).GetAwaiter().GetResult();
            Task.WhenAll(_altProducerTasks).GetAwaiter().GetResult();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBuffer()
        {
            using var readHandle = _queueBuffer.ReadBuffer();
            return readHandle.AsSpan();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferAlt()
        {
            using var readHandle = _altBuffer.ReadBuffer();
            return readHandle.AsSpan();
        }

        [Benchmark]
        public ReadOnlySpan<int> ArrayAsSpanBaseLine()
        {
            return _queueOutputPlaceholder;
        }
    }

    [GcServer(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    [AllStatisticsColumn]
    public class QueueBufferReadWriteOne
    {
        private readonly QueueBuffer<int>    _filledQueueBuffer;
        private readonly QueueBufferAlt<int> _filledQueueAltBuffer;
        private readonly int[]               _queueOutputPlaceholder = new int[N];

        private const int N = 1024;
        private       int _value;

        public QueueBufferReadWriteOne()
        {
            _filledQueueBuffer    = new QueueBuffer<int>(N);
            _filledQueueAltBuffer = new QueueBufferAlt<int>(N);
            _value                = new Random().Next();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBuffer()
        {
            _filledQueueBuffer.TryAdd(_value);
            using var readHandle = _filledQueueBuffer.ReadBuffer();
            return readHandle.AsSpan();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferAlt()
        {
            _filledQueueAltBuffer.TryAdd(_value);
            using var readHandle = _filledQueueAltBuffer.ReadBuffer();
            return readHandle.AsSpan();
        }

        [Benchmark]
        public ReadOnlySpan<int> ArrayAsSpanBaseLine()
        {
            _queueOutputPlaceholder[0] = _value;
            return _queueOutputPlaceholder;
        }
    }

    [GcServer(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    [AllStatisticsColumn]
    public class QueueBufferWrite
    {
        private readonly QueueBuffer<int>    _queueBuffer;
        private readonly QueueBufferAlt<int> _queueBufferAlt;

        private const int N = 1024;

        private int _writeValue;

        [Params(10, 100)]
        public int NumberOfItems;

        public QueueBufferWrite()
        {
            _queueBuffer    = new QueueBuffer<int>(N);
            _queueBufferAlt = new QueueBufferAlt<int>(N);
            _writeValue     = new Random().Next();
        }

        [Benchmark(Baseline = true)]
        public bool WriteQueueBuffer()
        {
            bool res = true;

            for (int i = 0; i < NumberOfItems; i++)
            {
                res &= _queueBuffer.TryAdd(_writeValue);
            }

            return res;
        }

        [Benchmark]
        public bool WriteQueueBufferAlt()
        {
            bool res = true;

            for (int i = 0; i < NumberOfItems; i++)
            {
                res &= _queueBufferAlt.TryAdd(_writeValue);
            }

            return res;
        }
    }


    [MemoryDiagnoser]
    public class ComplexAddRange
    {
        private const int InitialSize = 512;

        private FastList<Sample> _fastListToAdd;
        private List<Sample>     _listToAdd;

        [Params(128, 512)]
        public int NumberOfItems;

        [GlobalSetup]
        public void Setup()
        {
            var sample = new Sample(DateTimeOffset.UtcNow
                                  , Precision.Ms
                                  , 1
                                  , "X0012"
                                  , DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                  , "test");

            _fastListToAdd = new FastList<Sample>(512);
            _listToAdd     = new List<Sample>(512);

            for (int i = 0; i < NumberOfItems; i++)
            {
                _listToAdd.Add(sample);
                _fastListToAdd.Add(sample);
            }
        }

        [Benchmark]
        public int FastListAddRange()
        {
            using var fastList = new FastList<Sample>(InitialSize);
            fastList.AddRange(_fastListToAdd);
            fastList.AddRange(_fastListToAdd);
            return fastList.Count;
        }

        [Benchmark]
        public int FastListPooledAddRange()
        {
            using var fastListPooled = new FastList<Sample>(InitialSize, ArrayPool<Sample>.Shared);
            fastListPooled.AddRange(_fastListToAdd);
            fastListPooled.AddRange(_fastListToAdd);
            return fastListPooled.Count;
        }

        [Benchmark]
        public int ListAddRange()
        {
            var list = new List<Sample>(InitialSize);
            list.AddRange(_listToAdd);
            list.AddRange(_listToAdd);
            return list.Count;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _fastListToAdd.Dispose();
        }
    }

    [MemoryDiagnoser]
    [SimpleJob]
    public class FListVsListAddRange
    {
        private const int InitialCapacity = 128;

        [Benchmark(Baseline = true)]
        public int ListAddRange()
        {
            var list = new List<Point>(InitialCapacity);
            list.Add(new Point(1, 1));
            list.Add(new Point(2, 2));

            var listToAdd = new List<Point>(InitialCapacity);
            listToAdd.Add(new Point(3, 3));
            listToAdd.Add(new Point(4, 4));

            list.AddRange(listToAdd);

            return list.Count;
        }

        [Benchmark]
        public int FastListAddRangeNoPoolSpanCopy()
        {
            var list = new FastList<Point>(InitialCapacity);
            list.Add(new Point(1, 1));
            list.Add(new Point(2, 2));

            var listToAdd = new FastList<Point>(InitialCapacity);
            listToAdd.Add(new Point(3, 3));
            listToAdd.Add(new Point(4, 4));

            list.AddRange(listToAdd);

            return list.Count;
        }

        [Benchmark]
        public int FastListAddRangeDefaultPoolSpanCopy()
        {
            using var list = new FastList<Point>(InitialCapacity, ArrayPool<Point>.Shared);
            list.Add(new Point(1, 1));
            list.Add(new Point(2, 2));

            using var listToAdd = new FastList<Point>(InitialCapacity, ArrayPool<Point>.Shared);
            listToAdd.Add(new Point(3, 3));
            listToAdd.Add(new Point(4, 4));

            list.AddRange(listToAdd);

            return list.Count;
        }
    }

    [MemoryDiagnoser]
    public class RefIdxVsReadOnlySpanVsListToArray
    {
        private FastList<Point> _fastList;
        private List<Point>     _netList;

        [Params(128, 1024)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _netList  = new List<Point>(N);
            _fastList = new FastList<Point>(N);

            for (var i = 0; i < N; i++)
            {
                _netList.Add(new Point(i,  i));
                _fastList.Add(new Point(i, i));
            }
        }

        [Benchmark]
        public int FastListAsSpanUsageByIdx()
        {
            var count = _fastList.Count;
            var items = _fastList.AsSpan();
            int sum   = 0;

            for (int i = 0; i < count; i++)
            {
                ref var p = ref items[i];
                sum += p.X;
            }

            return sum;
        }

        [Benchmark]
        public int ListUsageByToArrayIdx()
        {
            var count   = _netList.Count;
            int sum     = 0;
            var asArray = _netList.ToArray();

            for (int i = 0; i < count; i++)
            {
                ref var p = ref asArray[i];
                sum += p.X;
            }

            return sum;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }
    }

    [MemoryDiagnoser]
    public class FListVsListCreate
    {
        [Params(128, 16384)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int InitialCapacity { get; set; }

        [Benchmark(Baseline = true)]
        public int ListCreate()
        {
            return new List<byte>(InitialCapacity).Capacity;
        }

        [Benchmark]
        public int FastListCreateNoPool()
        {
            return new FastList<byte>(InitialCapacity).Capacity;
        }

        [Benchmark]
        public int FastListCreateDefaultPool()
        {
            using var fastList = new FastList<byte>(InitialCapacity, ArrayPool<byte>.Shared);
            return fastList.Capacity;
        }
    }

#nullable enable
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public readonly struct Sample
    {
        public DateTimeOffset ts           { get; }
        public long           m_id         { get; }
        public string         asset_serial { get; }
        public long?          vi64         { get; }
        public Precision      p            { get; }
        public string         by_user      { get; }

        public Sample(DateTimeOffset ts
                    , long           measurementId
                    , string         assetSerial
                    , long?          vi64
                    , Precision      precision
                    , string         by
        )
        {
            this.ts      = ts;
            m_id         = measurementId;
            asset_serial = assetSerial;
            this.vi64    = vi64;
            p            = precision;
            by_user      = by;
        }

        public Sample(DateTimeOffset ts
                    , Precision      precision
                    , long           mId
                    , string         id
                    , long?          vi64
                    , string         by) : this(ts
                                              , mId
                                              , id
                                              , vi64
                                              , precision
                                              , by)
        {
        }
    }

    public enum Precision
    {
        Ns
      , Ms
      , S
    }
}