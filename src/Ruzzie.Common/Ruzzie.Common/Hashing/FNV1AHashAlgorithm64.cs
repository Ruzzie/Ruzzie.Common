using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Hashing
{
    /// <summary>
    /// An implementation of the FNV-1a hash. http://www.isthe.com/chongo/tech/comp/fnv/
    /// </summary>
    public class FNV1AHashAlgorithm64 : IHashCaseInsensitive64Algorithm
    {
        private static readonly TextInfo InvariantTextInfo = CultureInfo.InvariantCulture.TextInfo;
        const ulong FNVPrime64 = 1099511628211;
        const ulong FNVOffsetBasis64 = 14695981039346656037;

        /// <summary>
        /// Hashes the bytes.
        /// </summary>
        /// <param name="bytesToHash">The get bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the bytes are null</exception>
        public long HashBytes(byte[] bytesToHash)
        {
            if (ReferenceEquals(bytesToHash, null))
            {
                throw new ArgumentNullException(nameof(bytesToHash));
            }

            return HashBytesInternal(bytesToHash);
        }

        /// <summary>
        /// Hashes the string case insensitive.
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the string is null</exception>
        public long HashStringCaseInsensitive(string stringToHash)
        {
            if (ReferenceEquals(stringToHash, null))
            {
                throw new ArgumentNullException(nameof(stringToHash));
            }
            return GetInvariantCaseInsensitiveHashCode(stringToHash);
        }

#if !PORTABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static long HashBytesInternal(byte[] bytesToHash)
        {
            ulong hash = FNVOffsetBasis64;
            int byteCount = bytesToHash.Length;

            for (int i = 0; i < byteCount; ++i)
            {
                hash = HashByte(hash, bytesToHash[i]);
            }
            return (long)hash;
        }

#if !PORTABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static long GetInvariantCaseInsensitiveHashCode(string stringToHash)
        {
            ulong hash = FNVOffsetBasis64;
            int stringLength = stringToHash.Length;

            for (int i = 0; i < stringLength; ++i)
            {
                ushort currChar = InvariantTextInfo.ToUpper(stringToHash[i]);
                byte byteOne = (byte)currChar; //lower bytes              
                byte byteTwo = (byte)(currChar >> 8); //uppper byts

                hash = HashByte(HashByte(hash, byteOne), byteTwo);
            }
            return (long)hash;
        }

#if ! PORTABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static ulong HashByte(ulong currentHash, byte byteToHash)
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
        long HashBytes(byte[] bytesToHash);

        /// <summary>
        /// Hashes the string case insensitive.
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the string is null</exception>
        long HashStringCaseInsensitive(string stringToHash);
    }
}
