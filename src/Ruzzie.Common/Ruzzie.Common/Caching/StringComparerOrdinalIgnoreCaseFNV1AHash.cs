using Ruzzie.Common.Hashing;

namespace Ruzzie.Common.Caching;

/// <summary>
/// Equality comparer for strings that uses <see cref="StringComparer.OrdinalIgnoreCase"/> for equality and <see cref="FNV1AHashAlgorithm"/> for case insensitive string generating hashcodes.
/// </summary>
public class StringComparerOrdinalIgnoreCaseFNV1AHash : IEqualityComparer<string>
{
    private static readonly StringComparer     OrdinalIgnoreCaseComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly FNV1AHashAlgorithm HashAlgorithm             = new FNV1AHashAlgorithm();

    /// <summary>Determines whether the specified objects are equal.</summary>
    /// <returns>true if the specified objects are equal; otherwise, false.</returns>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    public bool Equals(string? x, string? y)
    {
        return OrdinalIgnoreCaseComparer.Equals(x, y);
    }

    /// <summary>Returns a hash code for the specified object.</summary>
    /// <returns>A hash code for the specified object.</returns>
    /// <param name="obj">The <see cref="T:System.String" /> for which a FNV1a hash code is to be returned.</param>
    /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
    public int GetHashCode(string obj)
    {
        return HashAlgorithm.HashStringCaseInsensitive(obj);
    }

    /// <summary>Returns a hash code for the specified object.</summary>
    /// <returns>A hash code for the specified object.</returns>
    /// <param name="value">The <see cref="T:System.String" /> for which a FNV1a hash code is to be returned.</param>
    /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="value" /> is a reference type and <paramref name="value" /> is null.</exception>
    public int GetHashCode(ReadOnlySpan<char> value)
    {
        return HashAlgorithm.HashStringCaseInsensitive(value);
    }
}