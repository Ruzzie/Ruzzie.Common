using System.Globalization;

namespace Ruzzie.Common.Hashing
{
    internal static class InvariantUpperCaseCharMap
    {
        static readonly char[] UpperCaseMap;
        static InvariantUpperCaseCharMap()
        {
            UpperCaseMap = new char[ushort.MaxValue+1];
            TextInfo invariantCultureTextInfo = CultureInfo.InvariantCulture.TextInfo;
         
            for (var i = 0; i < ushort.MaxValue+1; i++)
            {               
                UpperCaseMap[i] = invariantCultureTextInfo.ToUpper((char) i);
            }
        }

        public static char ToUpperInvariant(this char c)
        {
            return UpperCaseMap[c];
        }
    }
}