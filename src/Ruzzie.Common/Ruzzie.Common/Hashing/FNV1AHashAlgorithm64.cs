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
    private const ulong FNVPrime64 = 1099511628211;

    private const ulong FNVOffsetBasis64 = 14695981039346656037;
    // ReSharper restore ArrangeTypeMemberModifiers
    // ReSharper restore InconsistentNaming

    /// <inheritdoc />
    public long HashBytes(ReadOnlySpan<byte> bytesToHash)
    {
        return HashBytesInternal(bytesToHash);
    }

    /// <inheritdoc />
    public long HashStringCaseInsensitive(ReadOnlySpan<char> stringToHash)
    {
        return GetInvariantCaseInsensitiveHashCode(stringToHash);
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static long HashBytesInternal(ReadOnlySpan<byte> bytesToHash)
    {
        ulong hash      = FNVOffsetBasis64;
        int   byteCount = bytesToHash.Length;

        for (int i = 0; i < byteCount; ++i)
        {
            hash = HashByte(hash, bytesToHash[i]);
        }

        unchecked
        {
            return (long)hash;
        }
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static long GetInvariantCaseInsensitiveHashCode(ReadOnlySpan<char> stringToHash)
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
                    byte byteOne  = (byte)currChar;        //lower byte
                    byte byteTwo  = (byte)(currChar >> 8); //upper byte
                    hash = HashByte(HashByte(hash, byteOne), byteTwo);
                    currStr++;
                }
            }
        }

        unchecked
        {
            return (long)hash;
        }
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static ulong HashByte(ulong currentHash, byte byteToHash)
    {
        return (currentHash ^ byteToHash) * FNVPrime64;
    }
}