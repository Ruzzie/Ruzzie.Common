using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Hashing;

/// <inheritdoc />
/// <summary>
/// An implementation of the FNV-1a hash. http://www.isthe.com/chongo/tech/comp/fnv/
/// </summary>
// ReSharper disable once UnusedMember.Global
// ReSharper disable once InconsistentNaming
public class FNV1AHashAlgorithm64 : IHashCaseInsensitive64Algorithm
{
    // ReSharper disable ArrangeTypeMemberModifiers
    // ReSharper disable InconsistentNaming
    private const ulong FNVPrime64       = 1099511628211;
    private const ulong FNVOffsetBasis64 = 14695981039346656037;
    // ReSharper restore ArrangeTypeMemberModifiers
    // ReSharper restore InconsistentNaming

    /// <inheritdoc />
    /// <summary>
    /// Hashes the bytes.
    /// </summary>
    /// <param name="bytesToHash">The get bytes.</param>
    /// <returns></returns>
    /// <exception cref="T:System.ArgumentNullException">if the bytes are null</exception>
    public long HashBytes(in byte[] bytesToHash)
    {
        if (ReferenceEquals(bytesToHash, null))
        {
            throw new ArgumentNullException(nameof(bytesToHash));
        }

        return HashBytesInternal(bytesToHash);
    }

    /// <inheritdoc />
    /// <summary>
    /// Hashes the string case insensitive.
    /// </summary>
    /// <param name="stringToHash">The string to hash.</param>
    /// <returns></returns>
    /// <exception cref="T:System.ArgumentNullException">if the string is null</exception>
    public long HashStringCaseInsensitive(in string stringToHash)
    {
        if (ReferenceEquals(stringToHash, null))
        {
            throw new ArgumentNullException(nameof(stringToHash));
        }
        return GetInvariantCaseInsensitiveHashCode(stringToHash);
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static long HashBytesInternal(in byte[] bytesToHash)
    {
        ulong hash      = FNVOffsetBasis64;
        int   byteCount = bytesToHash.Length;

        for (int i = 0; i < byteCount; ++i)
        {
            hash = HashByte(hash, bytesToHash[i]);
        }
        return (long)hash;
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static long GetInvariantCaseInsensitiveHashCode(in string stringToHash)
    {
        ulong hash         = FNVOffsetBasis64;
        int   stringLength = stringToHash.Length;

        unsafe
        {
            fixed (char* pStr = stringToHash, pMap = InvariantUpperCaseStringExtensions.UpperCaseMap)
            {
                char* currStr = pStr;
                for (int i = 0; i < stringLength; ++i)
                {
                    var  currChar = pMap[*currStr];
                    byte byteOne  = (byte) currChar;        //lower bytes
                    byte byteTwo  = (byte) (currChar >> 8); //uppper byts
                    hash = HashByte(HashByte(hash, byteOne), byteTwo);
                    currStr++;
                }
            }
        }

        return (long)hash;
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static ulong HashByte(in ulong currentHash, in byte byteToHash)
    {
        return (currentHash ^ byteToHash) * FNVPrime64;
    }
}

/// <summary>
/// Interface for 64 bits case insensitive hashing
/// </summary>
public interface IHashCaseInsensitive64Algorithm
{
    /// <summary>
    /// Hashes the bytes.
    /// </summary>
    /// <param name="bytesToHash">The get bytes.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the bytes are null</exception>
    long HashBytes(in byte[] bytesToHash);

    /// <summary>
    /// Hashes the string case insensitive.
    /// </summary>
    /// <param name="stringToHash">The string to hash.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the string is null</exception>
    long HashStringCaseInsensitive(in string stringToHash);
}