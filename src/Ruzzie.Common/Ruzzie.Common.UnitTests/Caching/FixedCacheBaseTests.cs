using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ruzzie.Common.Caching;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Caching;

public abstract class FixedCacheBaseTests : CacheEfficiencyTests
{
    private readonly IFixedSizeCache<int, int> _flashCacheShouldCacheSameValues;

    protected FixedCacheBaseTests(ITestOutputHelper writer) : base(writer)
    {
        _flashCacheShouldCacheSameValues = CreateCache<int, int>(131072);
    }

    [Fact]
    public void Int32FixedSize()
    {
        CacheShouldStayFixedSize(i => i - (int.MaxValue / 2));
    }

    [Fact]
    public void StringFixedSize()
    {
        CacheShouldStayFixedSize(i => i.ToString().PadLeft(20, '0'));
    }

    protected void CacheShouldStayFixedSize<T>(Func<int, T> keyFactory, int? customCacheItemCountToAssert = null)
    {
        IFixedSizeCache<T, byte> cache = CreateCache<T, byte>(131072);
        int numberOfItemsToInsert = cache.MaxItemCount * 2; //add twice the items the cache can hold.
        for (var i = 0; i < numberOfItemsToInsert; i++)
        {
            cache.GetOrAdd(keyFactory.Invoke(i), _ => 1);
        }

        cache.CacheItemCount.Should()
             .BeLessOrEqualTo(customCacheItemCountToAssert ?? cache.MaxItemCount
                            , "Cache size does not seem limited by maxItemCount for: " + typeof(T));
    }

    [Fact]
    public void GetOrAddThrowsArgumentNullExceptionWhenKeyIsNull()
    {
        var act = () => CreateCache<string, string>(16).GetOrAdd("1", null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetOrAddShouldCacheItem()
    {
        IFixedSizeCache<string, int> cache               = CreateCache<string, int>(1024);
        var                          numberOfTimesCalled = 0;

        //Once
        cache.GetOrAdd("key"
                     , _ =>
                       {
                           numberOfTimesCalled++;
                           return numberOfTimesCalled;
                       });

        //Twice
        cache.GetOrAdd("key"
                     , _ =>
                       {
                           numberOfTimesCalled++;
                           return numberOfTimesCalled;
                       });

        numberOfTimesCalled.Should().Be(1);
    }

    [Theory]
    [InlineData(1,    1)]
    [InlineData(2,    2)]
    [InlineData(10,   10)]
    [InlineData(100,  98)]
    [InlineData(500,  480)] //misses due to poor hash spreading icm. pow 2
    [InlineData(1024, 980)] //misses due to poor hash spreading icm. pow 2
    public void CacheItemCountShouldReturnOnlyItemsInCache(int numberOfItemsToInsert, int expectedCount)
    {
        IFixedSizeCache<string, Guid> cache = CreateCache<string, Guid>(131072);

        for (var i = 0; i < numberOfItemsToInsert; i++)
        {
            Guid newGuid = Guid.NewGuid();
            cache.GetOrAdd(i + "CacheItemCountShouldReturnOnlyItemsInCache", _ => newGuid);
        }

        Debug.WriteLine(cache.MaxItemCount.ToString());
        cache.CacheItemCount.Should().BeGreaterOrEqualTo(expectedCount);
    }

    [Theory]
    [InlineData(1,    1)]
    [InlineData(2,    2)]
    [InlineData(10,   10)]
    [InlineData(100,  98)]  //misses due to poor hash spreading icm. pow 2
    [InlineData(500,  488)] //misses due to poor hash spreading icm. pow 2
    [InlineData(1024, 988)] //misses due to poor hash spreading icm. pow 2
    public void CacheItemCountShouldReturnOnlyItemsInCacheWithGuidAsKey(int numberOfItemsToInsert, int expectedCount)
    {
        IFixedSizeCache<Guid, Guid> cache = CreateCache<Guid, Guid>(131072);

        for (var i = 0; i < numberOfItemsToInsert; i++)
        {
            Guid newGuid = Guid.NewGuid();
            cache.GetOrAdd(newGuid, _ => newGuid);
        }

        Debug.WriteLine(cache.MaxItemCount.ToString());
        cache.CacheItemCount.Should().BeGreaterOrEqualTo(expectedCount);
    }

    [Fact]
    public void GetOrAddShouldReturnValueFactoryResult()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(1024);

        int value = cache.GetOrAdd("key", _ => 1);

        value.Should().Be(1);
    }

    [Fact]
    public void InitializeWithSizeLessThanOneShouldThrowArgumentException()
    {
        var act = () => CreateCache<int, int>(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OverwriteCacheLocationWithSameHash()
    {
        IFixedSizeCache<KeyWithConstantHash, int> flashCache = CreateCache<KeyWithConstantHash, int>(16);

        flashCache.GetOrAdd(new KeyWithConstantHash { Value = "10" }, _ => 10);
        int value = flashCache.GetOrAdd(new KeyWithConstantHash { Value = "99" }, _ => 99);

        value.Should().Be(99);
    }

    [Fact]
    public void ShouldCacheSameValue()
    {
        IFixedSizeCache<string, int> flashCache = CreateCache<string, int>(1024);

        flashCache.GetOrAdd("10", _ => 10);
        int value = flashCache.GetOrAdd("10", _ => 99);

        value.Should().Be(10);
    }


    [Theory]
    [InlineData(0,          0,          0)]
    [InlineData(1,          1,          1)]
    [InlineData(1024,       1024,       1024)]
    [InlineData(523,        523,        523)]
    [InlineData(1073741824, 1073741824, 1073741824)]
    public void ShouldCacheSameValues(int key, int valueToStore, int expected)
    {
        _flashCacheShouldCacheSameValues.GetOrAdd(key, _ => valueToStore);
        int value = _flashCacheShouldCacheSameValues.GetOrAdd(key, _ => 99);

        value.Should().Be(expected);
    }

    [Fact]
    public void ShouldInitializeWithComparer()
    {
        // ReSharper disable once ObjectCreationAsStatement
        CreateCache<string, string>(10, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldInitializeWithFixedMaximumSizeToNearestPowerOfTwo()
    {
        IFixedSizeCache<string, string> cache = CreateCache<string, string>(4194344);

        cache.MaxItemCount.Should().Be(4194304);
    }

    [Fact]
    public void ShouldInitializeWithPassedItemCount()
    {
        var cache = CreateCache<string, string>(8192);

        cache.MaxItemCount.Should().Be(8192);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(20)]
    [InlineData(100)]
    [InlineData(512)]
    [InlineData(31235)]
    public void MaxItemCountShouldBeLessOrEqualToGivenMaxItemCount(int givenMax)
    {
        IFixedSizeCache<string, string> cache = CreateCache<string, string>(givenMax);

        cache.MaxItemCount.Should().BeLessOrEqualTo(givenMax);
    }

    [Fact]
    public void InsertTwoItemsTest()
    {
        var keyOne = "No hammertime for: 19910";
        var keyTwo = "No hammertime for: 90063";

        IFixedSizeCache<string, string> cache = CreateCache<string, string>(1024);

        cache.GetOrAdd(keyOne, _ => keyOne).Should().Be(keyOne);
        cache.GetOrAdd(keyTwo, _ => keyTwo).Should().Be(keyTwo);
    }

    [Fact]
    public void TryGetShouldReturnTrueWhenValueIsInCache()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(1024);

        cache.GetOrAdd("1", _ => 1);

        cache.TryGet("1", out _).Should().BeTrue();
    }

    [Fact]
    public void TryGetShouldSetOutValueWhenValueIsInCache()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(1024);

        cache.GetOrAdd("1", _ => 1);
        cache.TryGet("1", out var value);

        value.Should().Be(1);
    }

    [Fact]
    public void TryGetShouldReturnFalseWhenValueIsNotInCache()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(1024);

        cache.TryGet("1", out _).Should().BeFalse();
    }

    [Fact]
    public void TryGetShouldSetOutValueToDefaultWhenValueIsNotInCache()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(1024);

        cache.TryGet("1", out var value);

        value.Should().Be(0);
    }

    [Fact]
    public void TryGetShouldReturnFalseForItemWithSameHashcodeAndDifferentValues()
    {
        IFixedSizeCache<KeyWithConstantHash, string> cache = CreateCache<KeyWithConstantHash, string>(1024);

        cache.GetOrAdd(new KeyWithConstantHash { Value = "a" }, _ => "1");

        cache.TryGet(new KeyWithConstantHash { Value = "b" }, out _).Should().BeFalse();
    }

    [Fact]
    public void MultiThreadedReadWriteTest()
    {
        IFixedSizeCache<string, string> cache =
            CreateCache<string, string>(131072, new StringComparerOrdinalIgnoreCaseFNV1AHash());
        int maxItemCount = cache.MaxItemCount;

        Parallel.For(0
                   , maxItemCount
                   , new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 8 }
                   , i =>
                     {
                         string key = i.ToString().PadLeft(20, 'C');

                         cache.GetOrAdd(key, _ => i.ToString()).Should().Be(i.ToString());

                         cache.GetOrAdd("A".PadLeft(20, '1'), _ => 42.ToString())
                              .Should()
                              .Be(42.ToString());
                     });

        //Cache size should be between The current item count * Efficiency and less than MaxItemCount
        cache.CacheItemCount.Should()
             .BeGreaterOrEqualTo((int)(maxItemCount * (MinimalEfficiencyInPercent / 100.0)))
             .And.BeLessOrEqualTo(maxItemCount);
    }

    [Theory]
    [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
    [InlineData(500)]
    public void MultiThreadedTest(int runtimeInMillis = 500)
    {
        IFixedSizeCache<string, byte> cache = null;

        //loop the cache size and assert a few times to check for bugs
        for (var k = 0; k < 5; k++)
        {
            cache = CreateCache<string, byte>(131072, StringComparer.OrdinalIgnoreCase);
            //first fill cache
            Parallel.For(0, cache.MaxItemCount, i => { cache.GetOrAdd(i.ToString().PadRight(20, 'M'), _ => 1); });

            //Cache size should be between The current item count * Efficiency and less than MaxItemCount
            cache.CacheItemCount.Should()
                 .BeGreaterOrEqualTo((int)(cache.MaxItemCount * (MinimalEfficiencyInPercent / 100.0)))
                 .And.BeLessOrEqualTo(cache.MaxItemCount);
        }

        var mustLoop = true;

        //write loops
        //Continuously write to the buffer
        Task writeLoopOne = Task.Run(() => { WriteToCacheLoop(ref mustLoop, cache); });

        Task writeLoopTwo = Task.Run(() => { WriteToCacheLoop(ref mustLoop, cache); });


        Thread.Sleep(runtimeInMillis);
        mustLoop = false;
        writeLoopOne.Wait();
        writeLoopTwo.Wait();

        //no exception should have occurred. and the size should be fixed

        //Cache size should be between The current item count * Efficiency and less than MaxItemCount
        cache!.CacheItemCount.Should()
              .BeGreaterOrEqualTo((int)(cache.MaxItemCount * (MinimalEfficiencyInPercent / 100.0)))
              .And.BeLessOrEqualTo(cache.MaxItemCount);
    }

    private static void WriteToCacheLoop(ref bool mustLoop, IFixedSizeCache<string, byte> cache)
    {
        Stopwatch timer   = new Stopwatch();
        var       counter = 0;
        timer.Start();
        while (mustLoop)
        {
            // ReSharper disable once PossibleNullReferenceException
            cache.GetOrAdd(counter.ToString().PadLeft(20, 'F'), _ => 1);
            counter++;
        }

        timer.Stop();

        string message = "Total write calls: " + counter;
        message += "\n" + "Avg timer per write call: " + timer.Elapsed.TotalMilliseconds     / counter + " ms.";
        message += "\n" + "Avg timer per write call: " + (double)(timer.Elapsed.Ticks * 100) / counter + " ns.";
        Trace.WriteLine(message);
    }

    [Fact]
    public void TrimShouldRemoveExcessEntriesWhenThereAreExcessEntries()
    {
        IFixedSizeCache<string, int> cache = CreateCache<string, int>(16);
        //overfill cache (that is why we use string, since the hashspreading in pow2 is poor

        for (var i = 0; i < cache.MaxItemCount * 2; i++)
        {
            int value = i;
            cache.GetOrAdd(i.ToString(), _ => value);
        }

        cache.CacheItemCount.Should()
             .BeGreaterOrEqualTo((int)(cache.MaxItemCount * (MinimalEfficiencyInPercent / 100.0)))
             .And.BeLessOrEqualTo(cache.MaxItemCount);
    }

    protected class KeyWithConstantHash
    {
        public string Value;

        public override int GetHashCode()
        {
            return 7;
        }

        protected bool Equals(KeyWithConstantHash other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((KeyWithConstantHash)obj);
        }

        public static bool operator ==(KeyWithConstantHash left, KeyWithConstantHash right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KeyWithConstantHash left, KeyWithConstantHash right)
        {
            return !Equals(left, right);
        }
    }
}