namespace Ruzzie.Common;

/// <summary>
/// Extension methods for strings.
/// </summary>
public static class StringExtensions
{

    /// <summary>
    /// Strips string of characters that are not [^a-zA-Z0-9 -]*
    /// </summary>
    /// <param name="str">The string to strip</param>
    /// <returns>The stripped string</returns>
    public static string StripAlternative(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty)
        {
            return "";
        }

        int length      = str.Length;
        int appendIndex = 0;

        const int MAX_STACK_LIMIT = 1024;

        var buffer = length <= MAX_STACK_LIMIT ? stackalloc char[length] : new char[length];

        for (int i = 0; i < length; ++i)
        {
            char c = str[i];
            // ReSharper disable RedundantCast
            if (97 <= (int) c && (int) c <= 122) //a-z
            {
                buffer[appendIndex] = c;
                appendIndex++;
            }
            else if (65 <= (int) c && (int) c <= 90) //A-Z
            {
                buffer[appendIndex] = c;
                appendIndex++;
            }
            else if (48 <= (int) c && (int) c <= 57) //0-9
            {
                buffer[appendIndex] = c;
                appendIndex++;
            }
            else if (32 == (int) c || 45 == (int) c) //space, -
            {
                buffer[appendIndex] = c;
                appendIndex++;
            }
            // ReSharper restore RedundantCast
        }

        return new string(buffer.Slice(0, appendIndex));
    }
}