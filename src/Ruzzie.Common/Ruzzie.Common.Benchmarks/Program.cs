using System;
using System.Buffers;
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
        private static void Main(string[] args)
        {
            var _ = BenchmarkRunner.Run(new[] { typeof(QueueBufferReadUnderPressure) });
        }
    }


    [GcServer(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    public class QueueBufferReadUnderPressure
    {
        private readonly QueueBufferSL<int> _queueBufferSl;
        private readonly QueueBufferSW<int> _queueBufferSW;

        private readonly int[] _queueOutputPlaceholder = new int[N];

        private const int N = 8388607 >> 7;


        private const    int    ConcurrentProducersCount = 2;
        private readonly Task[] _producerSLTasks         = new Task[ConcurrentProducersCount];
        private readonly Task[] _producerSWTasks         = new Task[ConcurrentProducersCount];

        public QueueBufferReadUnderPressure()
        {
            _queueBufferSl = new QueueBufferSL<int>(N);
            _queueBufferSW = new QueueBufferSW<int>(N);
        }

        private volatile bool _running;

        private void AddItemsSl()
        {
            int trivial = 0;

            while (_running)
            {
                bool addResult = _queueBufferSl.TryAdd(++trivial);
                if (addResult == false)
                    Thread.Sleep(0);
                /*else
                    Thread.Sleep(0);*/
            }
        }

        private void AddItemsSw()
        {
            int trivial = 0;

            while (_running)
            {
                bool addResult = _queueBufferSW.TryAdd(++trivial);
                if (addResult == false)
                    Thread.Sleep(0); // yield thread
                /*else
                    Thread.Sleep(0);*/
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(" // GLOBAL SETUP!");
            _running = true;
            for (int i = 0; i < ConcurrentProducersCount; i++)
            {
                _producerSLTasks[i] = Task.Run(AddItemsSl);
                _producerSWTasks[i] = Task.Run(AddItemsSw);
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Console.WriteLine(" // GLOBAL CLEANUP!");
            _running = false;
            Task.WhenAll(_producerSLTasks).GetAwaiter().GetResult();
            Task.WhenAll(_producerSWTasks).GetAwaiter().GetResult();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferSW()
        {
            using var readHandle = _queueBufferSW.ReadBuffer();
            return readHandle.Data;
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferSL()
        {
            using var readHandle = _queueBufferSl.ReadBuffer();
            return readHandle.Data;
        }

        [Benchmark]
        public ReadOnlySpan<int> ArrayAsSpanBaseLine()
        {
            return _queueOutputPlaceholder.AsSpan(0, 100);
        }
    }

    [GcServer(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    [AllStatisticsColumn]
    public class QueueBufferReadWriteOne
    {
        private readonly QueueBufferSL<int> _filledQueueBufferSl;
        private readonly QueueBufferSW<int> _filledQueueSwBuffer;
        private readonly int[]              _queueOutputPlaceholder = new int[N];

        private const int N = 1024;
        private       int _value;

        public QueueBufferReadWriteOne()
        {
            _filledQueueBufferSl = new QueueBufferSL<int>(N);
            _filledQueueSwBuffer = new QueueBufferSW<int>(N);
            _value               = new Random().Next();
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferSl()
        {
            _filledQueueBufferSl.TryAdd(_value);
            using var readHandle = _filledQueueBufferSl.ReadBuffer();
            return readHandle.Data;
        }

        [Benchmark]
        public ReadOnlySpan<int> ReadAllQueueBufferSw()
        {
            _filledQueueSwBuffer.TryAdd(_value);
            using var readHandle = _filledQueueSwBuffer.ReadBuffer();
            return readHandle.Data;
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
        private readonly QueueBufferSL<int> _queueBufferSl;
        private readonly QueueBufferSW<int> _queueBufferSw;

        private const int N = 1024;

        private Task _consumeTask;
        private Task _consumeAltTask;


        private int _writeValue;

        [Params(1, 10, 100)]
        public int NumberOfItems;

        public QueueBufferWrite()
        {
            _queueBufferSl = new QueueBufferSL<int>(N);
            _queueBufferSw = new QueueBufferSW<int>(N);
            _writeValue    = new Random().Next();
        }

        private volatile bool _running;

        public void ConsumeSl()
        {
            Span<int> data = new int[N];
            while (_running)
            {
                using var readHandle = _queueBufferSl.ReadBuffer();
                var       buffer     = readHandle.Data;
                for (int i = 0; i < buffer.Length; i++)
                {
                    data[i] = buffer[i];
                }
            }
        }

        public void ConsumeSw()
        {
            Span<int> data = new int[N];
            while (_running)
            {
                using var readHandle = _queueBufferSw.ReadBuffer();
                var       buffer     = readHandle.Data;
                for (int i = 0; i < buffer.Length; i++)
                {
                    data[i] = buffer[i];
                }
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(" // GLOBAL SETUP!");
            _running        = true;
            _consumeTask    = Task.Run(ConsumeSl);
            _consumeAltTask = Task.Run(ConsumeSw);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Console.WriteLine(" // GLOBAL CLEANUP!");
            _running = false;
            Task.WhenAll(_consumeTask, _consumeAltTask).GetAwaiter().GetResult();
        }

        [Benchmark(Baseline = true)]
        public bool WriteQueueBufferSL()
        {
            bool res = true;

            for (int i = 0; i < NumberOfItems; i++)
            {
                res &= _queueBufferSl.TryAdd(_writeValue);
            }

            return res;
        }

        [Benchmark]
        public bool WriteQueueBufferSw()
        {
            bool res = true;

            for (int i = 0; i < NumberOfItems; i++)
            {
                res &= _queueBufferSw.TryAdd(_writeValue);
            }

            return res;
        }
    }


    [MemoryDiagnoser]
    public class FastListComplexAddRange
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
    public class FastListRefIdxVsReadOnlySpanVsListToArray
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