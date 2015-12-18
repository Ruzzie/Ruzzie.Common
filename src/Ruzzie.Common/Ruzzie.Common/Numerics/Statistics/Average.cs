namespace Ruzzie.Common.Numerics.Statistics
{
    /// <summary>
    /// functions for average calculations
    /// </summary>
    public static class Average
    {
        /// <summary>
        ///     Streaming average calculation.
        /// </summary>
        /// <param name="previousAverage">The previous average.</param>
        /// <param name="currentNumber">The current number.</param>
        /// <param name="currentCount">The current count.</param>
        /// <returns>The new average</returns>
        public static double StreamAverage(double previousAverage, double currentNumber, double currentCount)
        {
            return ((previousAverage*currentCount) + currentNumber)/(currentCount + 1);
        }
    }
}