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
            UpperCaseMap = new char[ushort.MaxValue+1];
            TextInfo invariantCultureTextInfo = CultureInfo.InvariantCulture.TextInfo;
         
            for (var i = 0; i < ushort.MaxValue+1; i++)
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
        /// Converts the specified string to uppercase.
        /// </summary>
        /// <param name="str">The string to convert to uppercase</param>
        /// <returns>
        /// The specified string converted to uppercase.
        /// </returns>
        /// <exception cref="ArgumentNullException">str is null.</exception>
        public static string ToUpperInvariant(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            int strLength = str.Length;
#if !PORTABLE
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