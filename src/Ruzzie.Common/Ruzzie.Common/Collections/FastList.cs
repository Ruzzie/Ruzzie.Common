using System.Buffers;
using System.Runtime.CompilerServices;

#nullable enable
namespace Ruzzie.Common.Collections;

/// A base fast List (dynamic array) where you can access the underlying Span (Array).
/// This is optimized for scenario's where resizing is minimal and you want to reduce copying of data and still
///  need access to the underlying data via ref's.
/// Collection modification is not thread-safe. Please manage the lifecycle of this object with the Dispose method.
public sealed class FastList<T> : IMemoryOwner<T>
{
    private readonly ArrayPool<T>? _arrayPool;
    private          T[]           _array;
    private          int           _count;

    /// <summary>
    ///Creates a new <see cref="FastList{T}"/> with a minimum capacity of <paramref name="initialMinCapacity"/>.
    /// </summary>
    /// <param name="initialMinCapacity"></param>
    /// <param name="arrayPool">optional pool to use to allocate arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public FastList(int initialMinCapacity, ArrayPool<T>? arrayPool = null)
    {
        if (initialMinCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialMinCapacity));
        }

        _count     = 0;
        _arrayPool = arrayPool;
        _array     = AllocateArray(initialMinCapacity, arrayPool);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] AllocateArray(int minSize, ArrayPool<T>? arrayPool)
    {
        return arrayPool?.Rent(minSize) ?? new T[minSize];
    }

    /// add value
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        T[] array = _array;
        var size  = _count;
        if ((uint)size < (uint)array.Length)
        {
            _count      = size + 1;
            array[size] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        var size = _count;
        EnsureCapacity(size + 1);
        _count       = size + 1;
        _array[size] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int newCapacity)
    {
        if (newCapacity <= _array.Length)
        {
            return;
        }

        //this method can throw all kinds of exceptions,
        //  not sure about a proper error handling technique, 'yolo' style for now :(
        var oldArray = _array;

        var newSize = Math.Max(newCapacity, oldArray.Length * 2);

        var newArray = AllocateArray(newSize, _arrayPool);
        if (_count > 0)
        {
            Array.Copy(oldArray, newArray, _count);
        }

        _array = newArray;
        //Cleanup old array
        _arrayPool?.Return(oldArray);
    }

    /// Returns a <see cref="ReadOnlySpan{T}"/> of the underlying array.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(FastList<T> list)
    {
        return list.AsReadOnlySpan();
    }
    
    ///Attempts to copy the contents of this <see cref="FastList{T}"/> into a <see cref="Span{T}"/> and returns a value to indicate whether or not the operation succeeded.
    public bool TryCopyTo(Span<T> target)
    {
        return AsReadOnlySpan().TryCopyTo(target);
    }

    ///The number of items
    public int Count => _count;

    /// <inheritdoc />
    public void Dispose()
    {
        _arrayPool?.Return(_array);
        _array = null!; //force a dereference, is this the best way, I don't know..
    }

    /// <inheritdoc />
    public Memory<T> Memory => new Memory<T>(_array);

    ///Gets the total number of elements the internal data structure can hold without resizing.
    public int Capacity => _array.Length;

    /// Returns the data as a span. Be careful of the lifecycle of the List and multi-threading scenario's when modifying the collection.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(_array, 0, _count);
    }

    /// Returns the data as a readonly span. Be careful of the lifecycle of the List and multi-threading scenario's..
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan()
    {
        return new ReadOnlySpan<T>(_array, 0, _count);
    }

    /// Copies the contents of the given value to the end of the current list.
    public void AddRange(ReadOnlySpan<T> value)
    {
        var toAddSize = value.Length;

        if (toAddSize > 0)
        {
            //what to do with possible errors ...
            var currentSize = _count;
            var newSize     = currentSize + toAddSize;
            EnsureCapacity(newSize);

            var to = _array.AsSpan(_count..);
            value.TryCopyTo(to);

            _count = newSize;
        }
    }

    ///Copies the values of the given array to the end of this list.
    public void AddRange(T[] value)
    {
        AddRange(new ReadOnlySpan<T>(value));
    }

    /// Adds the elements of the given collection to the end of this list. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.
    public void AddRange(ICollection<T> otherCollection)
    {
        var toAddCount = otherCollection.Count;
        if (toAddCount > 0)
        {
            EnsureCapacity(_count + toAddCount);
            otherCollection.CopyTo(_array, _count);
            _count += toAddCount;
        }
    }

    /// Adds the elements of the given Enumerable to the end of this list.
    public void AddRange(IEnumerable<T> otherCollection)
    {
        if (otherCollection is ICollection<T> asCollection)
        {
            AddRange(asCollection);
        }
        else
        {
            using var en = otherCollection.GetEnumerator();
            while (en.MoveNext())
            {
                Add(en.Current);
            }
        }
    }
    
    /// <summary>
    /// Gets the element at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ref T this[int index]
    { 
        get
        {
            // This trick can reduce the range check by 1
            if ((uint)index >= (uint)_count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "out of bounds");
            }
            
            return ref _array[index];
        }
    }
}