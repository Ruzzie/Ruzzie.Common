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

            switch ((ushort)c)
            {
                //a-z
                case >= 97 and <= 122:
                //A-Z
                case >= 65 and <= 90:
                //0-9
                case >= 48 and <= 57:
                case 32:
                //space, -
                case 45:
                    buffer[appendIndex++] = c;
                    break;
            }
        }

        return new string(buffer.Slice(0, appendIndex));
    }
}