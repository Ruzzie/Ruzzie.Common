using System.Collections;

namespace Ruzzie.Common.Collections;

/// <summary>
/// Generic readonly wrapper for a <see cref="HashSet{T}"/>. Internally a <see cref="HashSet{T}"/> is used. All write operations throw a <see cref="NotSupportedException"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming"
                                               , "CA1710:IdentifiersShouldHaveCorrectSuffix"
                                               , Justification = "By design.")]
public class ReadOnlySet<T> : ISet<T>, IReadOnlySet<T>
{
    private readonly ISet<T> _wrappedSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlySet{T}" /> class.
    /// </summary>
    /// <param name="hashSet">The <see cref="HashSet{T}" /> to copy and wrap.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>A shallow copy of the <paramref name="hashSet"/> is made.</remarks>
    public ReadOnlySet(HashSet<T> hashSet)
    {
        if (hashSet == null)
        {
            throw new ArgumentNullException(nameof(hashSet));
        }

        _wrappedSet = new HashSet<T>(hashSet, hashSet.Comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlySet{T}"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <remarks>No shallow copy is made. If the underlying set is changed, this will be reflected is the <see cref="ReadOnlySet{T}"/>.</remarks>
    public ReadOnlySet(ISet<T> set)
    {
        if (set == null)
        {
            throw new ArgumentNullException(nameof(set));
        }

        _wrappedSet = set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlySet{T}"/> class by creating a <see cref="HashSet{T}"/> for the given <paramref name="collection"/>.
    /// If you want to use another set type use the <see cref="ReadOnlySet{T}(ISet{T})"/> constructor.
    /// </summary>
    /// <param name="collection">The collection to wrap, a <see cref="HashSet{T}"/> is used by default to wrap the collection.</param>
    public ReadOnlySet(IEnumerable<T> collection)
    {
        _wrappedSet = new HashSet<T>(collection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlySet{T}"/> class.
    /// </summary>
    /// <param name="collection">The collection to wrap, , a <see cref="HashSet{T}"/> is used by default to wrap the collection.</param>
    /// <param name="comparer">The comparer for equality comparisions of the values./>.</param>
    public ReadOnlySet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _wrappedSet = new HashSet<T>(collection, comparer);
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadOnly => true;

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _wrappedSet.GetEnumerator();
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public void Clear()
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// Determines whether the set [contains] [the specified item].
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///   <c>true</c> if the set contains [the specified item]; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(T item)
    {
        return _wrappedSet.Contains(item);
    }

    /// <summary>
    /// Copy items in this hashset to array, starting at arrayIndex
    /// </summary>
    /// <param name="array">array to add items to</param>
    /// <param name="arrayIndex">index to start at</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _wrappedSet.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public bool Remove(T item)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// NotSupported.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public bool Add(T item)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// NotSupported.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public void UnionWith(IEnumerable<T> other)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// NotSupported.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public void IntersectWith(IEnumerable<T> other)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// NotSupported.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public void ExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>
    /// NotSupported.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <exception cref="NotSupportedException">Set is a read only set.</exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException("Set is a read only set.");
    }

    /// <summary>Determines whether a set is a subset of a specified collection.</summary>
    /// <returns>true if the current set is a subset of <paramref name="other" />; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return _wrappedSet.IsSubsetOf(other);
    }

    /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
    /// <returns>true if the current set is a superset of <paramref name="other" />; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return _wrappedSet.IsSupersetOf(other);
    }

    /// <summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
    /// <returns>true if the current set is a proper superset of <paramref name="other" />; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set. </param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _wrappedSet.IsProperSupersetOf(other);
    }

    /// <summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
    /// <returns>true if the current set is a proper subset of <paramref name="other" />; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _wrappedSet.IsProperSubsetOf(other);
    }

    /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
    /// <returns>true if the current set and <paramref name="other" /> share at least one common element; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool Overlaps(IEnumerable<T> other)
    {
        return _wrappedSet.Overlaps(other);
    }

    /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
    /// <returns>true if the current set is equal to <paramref name="other" />; otherwise, false.</returns>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is null.</exception>
    public bool SetEquals(IEnumerable<T> other)
    {
        return _wrappedSet.SetEquals(other);
    }

    /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public int Count => _wrappedSet.Count;
}