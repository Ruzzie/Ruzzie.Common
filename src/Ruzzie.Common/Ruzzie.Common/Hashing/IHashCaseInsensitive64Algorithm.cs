namespace Ruzzie.Common.Hashing;

/// <summary>
/// Interface for 64 bits case insensitive hashing
/// </summary>
public interface IHashCaseInsensitive64Algorithm
{
    /// <summary>
    /// Hashes the bytes.
    /// </summary>
    /// <param name="bytesToHash">The bytes to hash</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the bytes are null</exception>
    long HashBytes(ReadOnlySpan<byte> bytesToHash);

    /// <summary>
    /// Hashes the string case insensitive.
    /// </summary>
    /// <param name="stringToHash">The string to hash.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the string is null</exception>
    long HashStringCaseInsensitive(ReadOnlySpan<char> stringToHash);
}