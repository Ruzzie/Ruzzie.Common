using System;

namespace Ruzzie.Common
{
    /// <summary>
    /// This class implements extension methods for the <see cref="System.Random"/> class.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns an array of random bytes.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="count">The size of the array to fill.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static byte[] NextBytes(this Random random, int count)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            byte[] samples = new byte[count];
            random.NextBytes(samples);
            return samples;           

        }
    }
}