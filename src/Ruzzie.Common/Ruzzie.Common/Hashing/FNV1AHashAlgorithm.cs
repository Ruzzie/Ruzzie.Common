using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Hashing;

/// <inheritdoc />
/// <summary>
/// An implementation of the FNV-1a hash. http://www.isthe.com/chongo/tech/comp/fnv/
/// </summary>
// ReSharper disable once UnusedMember.Global
// ReSharper disable once InconsistentNaming
public class FNV1AHashAlgorithm : IHashCaseInsensitiveAlgorithm
{
    // ReSharper disable InconsistentNaming
    private const uint FNVPrime32 = 16777619;

    private const uint FNVOffsetBasis32 = 2166136261;
    // ReSharper restore InconsistentNaming

    /// <inheritdoc />
    public int HashBytes(ReadOnlySpan<byte> bytesToHash)
    {
        return HashBytesInternal(bytesToHash);
    }

    /// <inheritdoc />
    public int HashStringCaseInsensitive(ReadOnlySpan<char> stringToHash)
    {
        return GetInvariantCaseInsensitiveHashCode(stringToHash);
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static int HashBytesInternal(ReadOnlySpan<byte> bytesToHash)
    {
        uint hash      = FNVOffsetBasis32;
        int  byteCount = bytesToHash.Length;

        for (int i = 0; i < byteCount; ++i)
        {
            hash = HashByte(hash, bytesToHash[i]);
        }

        unchecked
        {
            return (int)hash;
        }
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static int GetInvariantCaseInsensitiveHashCode(ReadOnlySpan<char> stringToHash)
    {
        uint hash         = FNVOffsetBasis32;
        int  stringLength = stringToHash.Length;

        unsafe
        {
            fixed (char* pStr = stringToHash, pMap = InvariantUpperCaseStringExtensions.UpperCaseMap)
            {
                char* currStr = pStr;
                for (int i = 0; i < stringLength; ++i)
                {
                    var  currChar = pMap[*currStr];
                    byte byteOne  = (byte)currChar;        //lower bytes
                    byte byteTwo  = (byte)(currChar >> 8); //upper bytes
                    hash = HashByte(HashByte(hash, byteOne), byteTwo);
                    currStr++;
                }
            }
        }

        unchecked
        {
            return (int)hash;
        }
    }

#if HAVE_METHODINLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static uint HashByte(uint currentHash, byte byteToHash)
    {
        return (currentHash ^ byteToHash) * FNVPrime32;
    }
}