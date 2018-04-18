using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Hashing
{
    /// <summary>
    /// An implementation of the FNV-1a hash. http://www.isthe.com/chongo/tech/comp/fnv/
    /// </summary>
    public class FNV1AHashAlgorithm : IHashCaseInsensitiveAlgorithm
    {
        // ReSharper disable InconsistentNaming
        private const uint FNVPrime32 = 16777619;        
        private const uint FNVOffsetBasis32 = 2166136261;
        // ReSharper restore InconsistentNaming
        /// <summary>
        /// Hashes the bytes.
        /// </summary>
        /// <param name="bytesToHash">The get bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the bytes are null</exception>
        public int HashBytes(in byte[] bytesToHash)
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
        public int HashStringCaseInsensitive(in string stringToHash)
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
        private static int HashBytesInternal(in byte[] bytesToHash)
        {
            uint hash = FNVOffsetBasis32;
            int byteCount = bytesToHash.Length;

            for (int i = 0; i < byteCount; ++i)
            {
                hash = HashByte(hash, bytesToHash[i]);
            }
            return (int)hash;
        }

#if !PORTABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static int GetInvariantCaseInsensitiveHashCode(in string stringToHash)
        {
            uint hash = FNVOffsetBasis32;
            int stringLength = stringToHash.Length;
#if !PORTABLE
            unsafe
            {
                fixed (char* pStr = stringToHash, pMap = InvariantUpperCaseStringExtensions.UpperCaseMap)
                {
                    char* currStr = pStr;
                    for (int i = 0; i < stringLength; ++i)
                    {                        
                        var currChar = pMap[*currStr];
                        byte byteOne = (byte)currChar; //lower bytes              
                        byte byteTwo = (byte)(currChar >> 8); //uppper byts                     
                        hash = HashByte(HashByte(hash, byteOne), byteTwo);
                        currStr++;                        
                    }
                }             
            }
#else
            for (int i = 0; i < stringLength; ++i)
            {
                ushort currChar = stringToHash[i].ToUpperInvariant();
                byte byteOne = (byte) currChar; //lower bytes              
                byte byteTwo = (byte) (currChar >> 8); //uppper byts

                hash = HashByte(HashByte(hash, byteOne), byteTwo);
            }
#endif

            return (int)hash;
        }

#if ! PORTABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint HashByte(in uint currentHash, in byte byteToHash)
        {
            return (currentHash ^ byteToHash) * FNVPrime32;
        }
    }

    /// <summary>
    /// Interface for case insensitive hashing
    /// </summary>
    public interface IHashCaseInsensitiveAlgorithm
    {
        /// <summary>
        /// Hashes the bytes.
        /// </summary>
        /// <param name="bytesToHash">The get bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the bytes are null</exception>
        int HashBytes(in byte[] bytesToHash);

        /// <summary>
        /// Hashes the string case insensitive.
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if the string is null</exception>
        int HashStringCaseInsensitive(in string stringToHash);
    }
}
