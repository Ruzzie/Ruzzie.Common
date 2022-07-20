namespace Ruzzie.Common.Numerics.Statistics;

/// <summary>
///     Histogram helper functions.
/// </summary>
public static class Histogram
{
    /// <summary>
    ///     Creates an ordered histogram for the given values.
    /// </summary>
    /// <typeparam name="T">The type of number to create the histogram for.</typeparam>
    /// <param name="values">The values to create the histogram from.</param>
    /// <returns>
    ///     A dictionary histogram, where the key is the value in <paramref name="values" /> and the value is the number
    ///     of occurrences.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="values" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     Gaps are not filled with zero occurrences, only values given with in the <paramref name="values" /> parameter
    ///     are used.
    /// </remarks>
    public static IOrderedEnumerable<KeyValuePair<T, int>> ToHistogramOrdered<T>(this IEnumerable<T> values) where T : IEquatable<T>, IComparable<T>
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }
        return values.ToHistogramDictionary().OrderBy(pair => pair.Key);
    }

    /// <summary>
    ///     Creates a histogram for the given values.
    /// </summary>
    /// <typeparam name="T">The type of number to create the histogram for.</typeparam>
    /// <param name="values">The values to create the histogram from.</param>
    /// <returns>
    ///     A dictionary histogram, where the key is the value in <paramref name="values" /> and the value is the number
    ///     of occurrences.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="values" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     Gaps are not filled with zero occurrences, only values given with in the <paramref name="values" /> parameter
    ///     are used.
    /// </remarks>
    public static IDictionary<T, int> ToHistogramDictionary<T>(this IEnumerable<T> values) where T : IEquatable<T>, IComparable<T>
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        IDictionary<T, int> histogram = new Dictionary<T, int>();
        foreach (T value in values)
        {
            if (histogram.ContainsKey(value) == false)
            {
                histogram[value] = 1;
            }
            else
            {
                histogram[value] += 1;
            }
        }

        return histogram;
    }
}