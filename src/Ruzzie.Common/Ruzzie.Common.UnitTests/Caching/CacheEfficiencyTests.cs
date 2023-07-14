using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Ruzzie.Common.Caching;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Caching;

[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
[Collection("CacheEfficiencyTests")]
public abstract class CacheEfficiencyTests
{
    protected abstract IFixedSizeCache<TKey, TValue> CreateCache<TKey, TValue>(
        int                     maxItemCount
      , IEqualityComparer<TKey> comparer = null);


    public CacheEfficiencyTests(ITestOutputHelper outWriter)
    {
        Out = outWriter;
    }

    protected readonly ITestOutputHelper Out;

    protected abstract double MinimalEfficiencyInPercent { get; }

    [Fact]
    public void BooleanEfficiency()
    {
        CacheEfficiencyShouldBe<bool>(i => i % 2 == 0, 2);
    }

    [Fact]
    public void ByteEfficiency()
    {
        CacheEfficiencyShouldBe<byte>(i => (byte)(i - byte.MaxValue / 2), 256);
    }

    [Fact]
    public void SByteEfficiency()
    {
        CacheEfficiencyShouldBe<sbyte>(i => (sbyte)(i - sbyte.MaxValue / 2), 256);
    }

    [Fact]
    public void Int16Efficiency()
    {
        CacheEfficiencyShouldBe<short>(i => (short)(i - short.MaxValue / 2));
    }

    [Fact]
    public void UInt16Efficiency()
    {
        CacheEfficiencyShouldBe<ushort>(i => (ushort)(i - ushort.MaxValue / 2));
    }

    [Fact]
    public void Int32Efficiency()
    {
        CacheEfficiencyShouldBe<int>(i => i - int.MaxValue / 2);
    }

    [Fact]
    public void UInt32Efficiency()
    {
        CacheEfficiencyShouldBe<uint>(i => (uint)i - uint.MaxValue / 2);
    }

    [Fact]
    public void Int64Efficiency()
    {
        CacheEfficiencyShouldBe<long>(i => i - long.MaxValue / 2);
    }

    [Fact]
    public void UInt64Efficiency()
    {
        CacheEfficiencyShouldBe<ulong>(i => (ulong)i - ulong.MaxValue / 2);
    }

    [Fact]
    public void FloatEfficiency()
    {
        CacheEfficiencyShouldBe<float>(i => i);
    }

    [Fact]
    public void DoubleEfficiency()
    {
        CacheEfficiencyShouldBe<double>(i => i / 2.0 * 10000.0);
    }

    [Fact]
    public void DecimalEfficiency()
    {
        CacheEfficiencyShouldBe<decimal>(i => i / 2.0m /* * 10000.0m*/);
    }

    [Fact]
    public void CharEfficiency()
    {
        CacheEfficiencyShouldBe<char>(i => (char)(i - char.MaxValue / 2));
    }

    [Fact]
    public void DateEfficiency()
    {
        CacheEfficiencyShouldBe<DateTime>(
                                          i => new DateTime(i % 9998 + 1
                                                          , i % 11   + 1
                                                          , i % 27   + 1
                                                          , i % 23   + 1
                                                          , i % 59   + 1
                                                          , i % 59   + 1));
    }

    [Fact]
    public void StringWithFNV1AOrdinalIgnoreCaseEfficiency()
    {
        CacheEfficiencyShouldBe<string>(i => i.ToString().PadLeft(20, '0')
                                      , comparer: new StringComparerOrdinalIgnoreCaseFNV1AHash());
    }

    [Fact]
    public void StringWithDefaults()
    {
        CacheEfficiencyShouldBe<string>(i => i.ToString().PadLeft(20, '0'));
    }

    [Fact]
    public void GuidEfficiency()
    {
        CacheEfficiencyShouldBe<Guid>(_ => Guid.NewGuid());
    }

    [Fact]
    public void TimespanEfficiency()
    {
        CacheEfficiencyShouldBe<TimeSpan>(i => TimeSpan.FromSeconds(i + i * i));
    }

    public void CacheEfficiencyShouldBe<T>(Func<int, T>         keyFactory
                                         , int?                 customCacheItemCountToAssert = null
                                         , IEqualityComparer<T> comparer                     = null)
    {
        IFixedSizeCache<T, byte> cache                 = CreateCache<T, byte>(32768, comparer);
        int                      numberOfItemsToInsert = cache.MaxItemCount;

        for (int i = 0; i < numberOfItemsToInsert; i++)
        {
            cache.GetOrAdd(keyFactory.Invoke(i), _ => 1);
        }

        double efficiency = (double)cache.CacheItemCount / cache.MaxItemCount * 100.0;


        Out.WriteLine($"<{typeof(T).Name}> ({(comparer == null ? "DefaultComparer" : comparer.GetType().Name)}) Cache efficiency is: {efficiency:F2}");
        if (customCacheItemCountToAssert != null)
        {
            cache.CacheItemCount.Should()
                 .Be(customCacheItemCountToAssert, $"Cache efficiency is not 100 percent for type: {typeof(T)}");
        }
        else
        {
            efficiency.Should()
                      .BeGreaterOrEqualTo(MinimalEfficiencyInPercent
                                        , $"Cache efficiency is not {MinimalEfficiencyInPercent:F2} percent for type: {typeof(T)}");
        }
    }
}