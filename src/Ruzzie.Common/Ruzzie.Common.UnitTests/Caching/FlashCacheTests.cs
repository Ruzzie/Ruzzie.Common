using System.Collections.Generic;
using Ruzzie.Common.Caching;

namespace Ruzzie.Common.UnitTests.Caching;

// ReSharper disable once UnusedType.Global
public class FlashCacheTests : FixedCacheBaseTests
{
    protected override double MinimalEfficiencyInPercent => 62;

    protected override IFixedSizeCache<TKey, TValue> CreateCache<TKey, TValue>(
        int                     maxItemCount
      , IEqualityComparer<TKey> equalityComparer = null)
    {
        return new FlashCache<TKey, TValue>(equalityComparer, maxItemCount);
    }
}