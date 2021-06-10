using System;
using System.Globalization;

namespace Ruzzie.Common.Hashing
{
    /// <summary>
    /// Optimized string methods for invariant string.
    /// </summary>
    public static class InvariantUpperCaseStringExtensions
    {
        internal static readonly char[] UpperCaseMap;

        static InvariantUpperCaseStringExtensions()
        {
            int maxIndexOfUpperCaseMap = char.MaxValue;
            UpperCaseMap = new char[maxIndexOfUpperCaseMap + 1];
            var invariantCultureTextInfo = CultureInfo.InvariantCulture.TextInfo;

            for (int i = maxIndexOfUpperCaseMap; i >= 0; i--)
            {
                UpperCaseMap[i] = invariantCultureTextInfo.ToUpper((char) i);
            }
        }

        /// <summary>
        /// Converts the specified character to uppercase.
        /// </summary>
        /// <param name="c">The character to convert to uppercase.</param>
        /// <returns>The specified character converted to uppercase.</returns>
        public static char ToUpperInvariant(this char c)
        {
            return UpperCaseMap[c];
        }

        /// <summary>
        /// Modifies the buffer to uppercase.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        [CLSCompliant(false)]
        public static unsafe void ToUpperInvariant(char* buffer, int startIndex, int length)
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

            fixed (char* pMap = UpperCaseMap)
            {
                for (int i = startIndex; i < length; i++)
                {
                    char sourceChar = buffer[i];
                    buffer[i] = pMap[sourceChar];
                }
            }
        }

        /// <summary>
        /// Modifies the buffer to uppercase.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        public static unsafe void ToUpperInvariant(char[] buffer, int startIndex, int length)
        {
            if (length <= 0)
            {
                return;
            }

            if (startIndex + length > buffer.Length)
            {
                return;
            }

            fixed (char* pMap = UpperCaseMap)
            {
                for (int i = startIndex; i < length; i++)
                {
                    char sourceChar = buffer[i];
                    buffer[i] = pMap[sourceChar];
                }
            }
        }

        /// <summary>
        /// Converts the specified string to uppercase.
        /// </summary>
        /// <param name="str">The string to convert to uppercase</param>
        /// <returns>
        /// The specified string converted to uppercase.
        /// </returns>
        /// <exception cref="ArgumentNullException">str is null.</exception>
        public static string ToUpperInvariant(in string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            int strLength = str.Length;

            unsafe
            {
                char* pTarget = stackalloc char[strLength];
                fixed (char* pSource = str, pMap = UpperCaseMap)
                {
                    char* pSourceChar = pSource;
                    for (int i = 0; i < strLength; i++)
                    {
                        char sourceChar = *pSourceChar;
                        pTarget[i] = pMap[sourceChar];
                        pSourceChar++;
                    }
                }

                return new string(pTarget, 0, strLength);
            }
        }
    }
}