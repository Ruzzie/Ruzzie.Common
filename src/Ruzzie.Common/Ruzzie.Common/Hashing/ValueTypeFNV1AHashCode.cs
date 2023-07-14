using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Hashing;

internal static class FNV1AHashCodeUnsafe<T>
{
    private static readonly int SizeInBytes = Unsafe.SizeOf<T>();

    /// Calculates the hashcode directly from the bytes in memory where the number of bytes is sizeof(T)
    ///    beware this is not safe ...
    public static unsafe int HashCode(ref T value)
    {
        // basically we hash the byte values in memory of the given T
        //   so a Double would hash the 4 byte values
        // The BitConverter.GetBytes() has the same result
        //   only this allocates a new array, and here we just pass a pointer

        return FNV1AHashAlgorithm.Hash(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref value), SizeInBytes));
    }
}

/// Can be used to very quickly get a FNV1A type hashcode for value types, where it just hashes the bytes in memory.
public static class ValueTypeFNV1AHashCode<T> where T : struct
{
    /// Calculates the hashcode
    public static int HashCode(ref T value)
    {
        return FNV1AHashCodeUnsafe<T>.HashCode(ref value);
    }
}

/// <summary>
/// Hashcode calculation for T.
/// Uses FNV1A Hashcode for ValueTypes and Strings and uses a given <see cref="IEqualityComparer{T}"/> as fallback for reference types.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class FNV1AHashcodeOrDefaultComparer<T> : IEqualityComparer<T>
{
    /// The given fallback comparer which we use for hashcode calculations
    private readonly IEqualityComparer<T> _fallBack;

    /// indicates which hash type to use
    private readonly UseHashType _hashType;


    /// Creates a new <see cref="FNV1AHashcodeOrDefaultComparer{T}"/> and optimized initialization for the given type.
    public FNV1AHashcodeOrDefaultComparer(IEqualityComparer<T>? fallBack)
    {
        _fallBack = fallBack ?? EqualityComparer<T>.Default;

        var type = typeof(T);

        var hashType = UseHashType.Default;

        if (type.IsValueType)
            hashType = UseHashType.ValueType;

        _hashType = hashType;
    }

    /// Indicates what hash calculation type we need to use
    private enum UseHashType
    {
        /// Use the default or given 
        Default

       ,

        /// We can use an optimized version to calculate hash codes <see cref="ValueTypeFNV1AHashCode{T}"/>
        ValueType
    }

    /// <inheritdoc />
    public bool Equals(T? x, T? y)
    {
        return _fallBack.Equals(x, y);
    }

    /// <summary>
    /// Calculates the FNV1A hashcode for value types, or uses the given fallback comparer for reference types
    /// </summary>
    public int GetHashCode([DisallowNull] T obj)
    {
        switch (_hashType)
        {
            case UseHashType.ValueType:
                return FNV1AHashCodeUnsafe<T>.HashCode(ref obj);
            default:
            case UseHashType.Default:
                return _fallBack.GetHashCode(obj);
        }
    }
}