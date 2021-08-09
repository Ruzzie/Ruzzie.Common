using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Ruzzie.Common.Collections;

namespace Ruzzie.Common.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var allBenchmarks = BenchmarkRunner.Run(typeof(Program).Assembly);
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