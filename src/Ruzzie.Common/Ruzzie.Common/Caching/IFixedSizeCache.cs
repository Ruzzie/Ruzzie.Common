﻿namespace Ruzzie.Common.Caching;

/// <summary>
/// Interface for a memory cache that has a fixed size.
/// </summary>
/// <typeparam name="TKey">The cache key.</typeparam>
/// <typeparam name="TValue">The value to cache</typeparam>
public interface IFixedSizeCache<TKey, TValue>
{
    /// <summary>
    /// The current numbers of items that are cached.
    /// </summary>
    int CacheItemCount { get; }

    /// <summary>
    /// Gets the potentially maximum number of items the cache can hold.
    /// </summary>
    /// <value>
    /// The maximum item count.
    /// </value>
    int MaxItemCount { get; }

    /// <summary>
    ///     Get an item for the given key. Or add them using the given value factory.
    /// </summary>
    /// <param name="key">The key to store the value for. Key can be null or default.</param>
    /// <param name="valueFactory">
    ///     The function that generated the value to store. This will only be executed when the key is
    ///     not yet present.
    /// </param>
    /// <returns>
    ///     The value. If it was cached the cached value is returned. If it was not cached the value from the value
    ///     factory is returned.
    /// </returns>
    TValue GetOrAdd(in TKey key, in Func<TKey, TValue> valueFactory);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="cacheKey">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the <see cref="FlashCache{TKey,TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    /// <remarks>If the key is not found, then the value parameter gets the appropriate default value for the type TValue; for example, 0 (zero) for integer types, false for Boolean types, and null for reference types. </remarks>
    bool TryGet(in TKey cacheKey, out TValue value);
}