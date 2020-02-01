using System;
using System.Globalization;

namespace Ruzzie.Common.Hashing
{
    /// <summary>
    /// Optimized string methods for invariant string.
    /// </summary>
    public static class InvariantLowerCaseStringExtensions
    {
        internal static readonly char[] LowerCaseMap;
        static InvariantLowerCaseStringExtensions()
        {
            int maxIndexOfLowerCaseMap = char.MaxValue;
            LowerCaseMap = new char[maxIndexOfLowerCaseMap+1];
            var invariantCultureTextInfo = CultureInfo.InvariantCulture.TextInfo;
         
            for (int i = maxIndexOfLowerCaseMap; i >= 0; i--)
            {               
                LowerCaseMap[i] = invariantCultureTextInfo.ToLower((char) i);
            }
        }

        /// <summary>
        /// Converts the specified character to lowercase.
        /// </summary>
        /// <param name="c">The character to convert to lowercase.</param>
        /// <returns>The specified character converted to lowercase.</returns>
        public static char ToLowerInvariant(this char c)
        {
            return LowerCaseMap[c];
        }

        /// <summary>
        /// Modifies the buffer to lowercase.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        [CLSCompliant(false)]
        public static unsafe void ToLowerInvariant(char* buffer, int startIndex, int length)
        {
            if (length <= 0)
            {
                return;
            }

            if (buffer + startIndex < buffer)
            {
                // This means that the pointer operation has had an overflow
                return;
            }

            fixed (char* pMap = LowerCaseMap)
            {
                for (int i = startIndex; i < length; i++)
                {
                    char sourceChar = buffer[i];
                    buffer[i] = pMap[sourceChar];
                }
            }
        }

        /// <summary>
        /// Modifies the buffer to lowercase.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        public static unsafe void ToLowerInvariant(char[] buffer, int startIndex, int length)
        {
            if (length <= 0)
            {
                return;
            }

            if (startIndex + length > buffer.Length)
            {
                return;
            }

            fixed (char* pMap = LowerCaseMap)
            {
                for (int i = startIndex; i < length; i++)
                {
                    char sourceChar = buffer[i];
                    buffer[i] = pMap[sourceChar];
                }
            }
        }

        /// <summary>
        /// Converts the specified string to lowercase.
        /// </summary>
        /// <param name="str">The string to convert to lowercase</param>
        /// <returns>
        /// The specified string converted to lowercase.
        /// </returns>
        /// <exception cref="ArgumentNullException">str is null.</exception>
        public static string ToLowerInvariant(in string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            int strLength = str.Length;
            
#if !PORTABLE
            unsafe
            {
                var pTarget = stackalloc char[strLength];
                fixed (char* pSource = str, pMap = LowerCaseMap)
                {
                    char* pCurrSourceChar = pSource;
                    for (int i = 0; i < strLength; i++)
                    {
                        char sourceChar = *pCurrSourceChar;                        
                        pTarget[i] = pMap[sourceChar];
                        pCurrSourceChar++;
                    }
                }
                return new string(pTarget, 0, strLength);
            }
#else
            char[] newStr = new char[strLength];
            for (int i = 0; i < strLength; i++)
            {
                newStr[i] = str[i].ToUpperInvariant();
            }
            return new string(newStr);
#endif  
        }
    }
}