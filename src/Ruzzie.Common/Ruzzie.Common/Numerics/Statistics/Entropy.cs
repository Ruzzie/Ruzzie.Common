using System;
using System.Collections.Generic;

namespace Ruzzie.Common.Numerics.Statistics
{
    /// <summary>
    /// Functions for calculating entropy
    /// </summary>
    public static class Entropy
    {
        /// <summary>
        ///     Calculates the entropy.
        /// </summary>
        /// <typeparam name="T">The type of number to calculate entropy for.</typeparam>
        /// <param name="histogram">The histogram with the data</param>
        /// <param name="sampleSize">The total number of samples.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="histogram"/> is <see langword="null" />.</exception>
        public static double CalculateEntropy<T>(this IDictionary<T, int> histogram, int sampleSize)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException(nameof(histogram));
            }
                    
            return CalculateEntropy(histogram as IEnumerable<KeyValuePair<T, int>>, sampleSize);
        }

        /// <summary>
        ///     Calculates the entropy.
        /// </summary>
        /// <typeparam name="T">The type of number to calculate entropy for.</typeparam>
        /// <param name="histogram">The histogram with the data</param>
        /// <param name="sampleSize">The total number of samples.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="histogram"/> is <see langword="null" />.</exception>
        public static double CalculateEntropy<T>(this IEnumerable<KeyValuePair<T, int>> histogram, int sampleSize)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException(nameof(histogram));
            }

            double entropy = 0;
            foreach (KeyValuePair<T, int> numberCount in histogram)
            {
                double probability = (double)numberCount.Value / sampleSize;
                entropy += probability * Math.Log(1 / probability, 2);
            }
            return entropy;
        }
    }
}