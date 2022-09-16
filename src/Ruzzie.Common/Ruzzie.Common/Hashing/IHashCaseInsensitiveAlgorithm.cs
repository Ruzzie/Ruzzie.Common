namespace Ruzzie.Common.Hashing;

/// <summary>
/// Interface for case insensitive hashing
/// </summary>
public interface IHashCaseInsensitiveAlgorithm
{
    /// <summary>
    /// Hashes the bytes.
    /// </summary>
    /// <param name="bytesToHash">The bytes to hash</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the bytes are null</exception>
    int HashBytes(ReadOnlySpan<byte> bytesToHash);

    /// <summary>
    /// Hashes the string case insensitive.
    /// </summary>
    /// <param name="stringToHash">The string to hash.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">if the string is null</exception>
    int HashStringCaseInsensitive(ReadOnlySpan<char> stringToHash);
}