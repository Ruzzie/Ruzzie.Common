using System.Runtime.CompilerServices;

namespace Ruzzie.Common;

/// Encapsulate a Value type to pass as a reference
///   this can be used for shared access (pass a pointer to a value)
public sealed class Ref<T> where T : struct
{
    /// Read or write the value
    public T Value;

    /// <summary>
    /// Creates a new <see cref="Ref{T}"/> that is initialized with the passed value.
    /// </summary>
    /// <param name="value">The initial value</param>
    public Ref(T value)
    {
        Value = value;
    }
}

///A boolean (ref) that has thread-safe atomic operations
public sealed class AtomicBool
{
    private const long TRUE_VALUE  = 1;
    private const long FALSE_VALUE = 0;
    private       long _value;

    /// Creates a new <see cref="AtomicBool"/> with the given initial value
    public AtomicBool(bool value)
    {
        WriteUnfenced(value);
    }

    /// Creates a new <see cref="AtomicBool"/> with the default value of false
    public AtomicBool()
    {
        _value = FALSE_VALUE;
    }

    /// Reads Atomically
    public bool ReadAtomic()
    {
        return Interlocked.Read(ref _value) == TRUE_VALUE;
    }

    ///Write Atomically
    public void WriteAtomic(bool value)
    {
        Interlocked.Exchange(ref _value, BoolToLong(value));
    }

    /// Volatile read
    ///<remarks>Reads the value. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</remarks>
    public bool ReadVolatile()
    {
        return Volatile.Read(ref _value) == TRUE_VALUE;
    }


    /// <summary>
    /// Reads the value applying a compiler only fence, no CPU fence is applied
    /// </summary>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public bool ReadCompilerFenced()
    {
        return _value == TRUE_VALUE;
    }


    /// Writes the given value unfenced, thread-safety and / or atomicity are NOT guaranteed.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnfenced(bool value)
    {
        _value = BoolToLong(value);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long BoolToLong(bool value)
    {
        if (value)
            return TRUE_VALUE;
        else
            return FALSE_VALUE;
    }
}