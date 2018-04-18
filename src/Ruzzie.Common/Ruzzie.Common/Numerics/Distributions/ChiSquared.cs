using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ruzzie.Common.Numerics.Statistics;
#if !PORTABLE
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
#endif

namespace Ruzzie.Common.Numerics.Distributions
{
    /// <summary>
    /// Methods for ChiSquared calculation
    /// </summary>
    public static class ChiSquared
    {
#region adapted from entlib chisq.c
        /*
            Module:       chisq.c
            Purpose:      compute approximations to chisquare distribution probabilities
            Contents:     pochisq()
            Uses:         poz() in z.c (Algorithm 209)
            Programmer:   Gary Perlman
            Organization: Wang Institute, Tyngsboro, MA 01879
            Copyright:    none
        */

        /// <summary>
        /// log (sqrt (pi))
        /// </summary>
        private const double LogSqrtPi = 0.5723649429247000870717135;
        /// <summary>
        ///  1 / sqrt (pi)
        /// </summary>
        private const double SqrtPi = 0.5641895835477562869480795;
        /// <summary>
        /// max value to represent exp (x) 
        /// </summary>
        private const double BigX = 20.0;

        private static double Ex(in double x)
        {            
            return (((x) < -BigX) ? 0.0 : Math.Exp(x));
        }
       
        /// <summary>
        /// probability of chi square value
        /// </summary>
        /// <param name="ax">obtained chi-square value</param>
        /// <param name="degreesOfFreedom">degrees of freedom</param>
        /// <returns> probability of chi square value</returns>
        /// <remarks>
        /// Adapted from:
        ///        Hill, I.D.and Pike, M. C.Algorithm 299
        ///        Collected Algorithms for the CACM 1967 p. 243
        ///    Updated for rounding errors based on remark in
        ///        ACM TOMS June 1985, page 185
        /// </remarks>
        public static double ProbabilityOfChiSquared(in double ax, in int degreesOfFreedom)
        {
            double x = ax;
            double y = 0;
            double a, s;

            double e, c, z;
            bool even; /* true if df is an even number */

            if (x <= 0.0 || degreesOfFreedom < 1)
            {
                return 1.0;
            }

            a = 0.5 * x;
            even = (2 * (degreesOfFreedom / 2)) == degreesOfFreedom;
            if (degreesOfFreedom > 1)
            {
                y = Ex(-a);
            }
            s = (even ? y : (2.0*ZProbability.ProbabilityOfZ(-Math.Sqrt(x))));
            if (degreesOfFreedom > 2)
            {
                x = 0.5 * (degreesOfFreedom - 1.0);
                z = (even ? 1.0 : 0.5);
                if (a > BigX)
                {
                    e = (even ? 0.0 : LogSqrtPi);
                    c = Math.Log(a);
                    while (z <= x)
                    {
                        e = Math.Log(z) + e;
                        s += Ex(c * z - a - e);
                        z += 1.0;
                    }
                    return (s);
                }
                else
                {
                    e = (even ? 1.0 : (SqrtPi / Math.Sqrt(a)));
                    c = 0.0;
                    while (z <= x)
                    {
                        e = e * (a / z);
                        c = c + e;
                        z += 1.0;
                    }
                    return (c * y + s);
                }
            }
            else
            {
                return s;
            }
        }
        #endregion

        /// <summary>
        ///  Chi squared P of samples.
        /// </summary>
        /// <param name="sampleSize">Size of the sample.</param>
        /// <param name="samples">The samples.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The <paramref name="samples"/> is an empty collection.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="sampleSize"/> argument is less than or equal to 0..</exception>
        /// <exception cref="ArgumentNullException"><paramref name="samples"/> is <see langword="null" />.</exception>
        [SuppressMessage("ReSharper", "RedundantCast")]
        public static double ChiSquaredP(in int sampleSize, in byte[] samples)
        {
            if (sampleSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSize));
            }

            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }
         
            if (samples.Length == 0)
            {
                throw new ArgumentException("Argument is an empty collection", nameof(samples));
            }

            const int byteMaxValuePlusOne = byte.MaxValue + 1;

            double expectedCount = (double) sampleSize/ byteMaxValuePlusOne;
            var histogram = samples.ToHistogramDictionary();

            double chisq = 0;
            for (int i = 0; i < byteMaxValuePlusOne; i++)
            {
                if (histogram.TryGetValue((byte) i, out var intValue) == false)
                {
                    intValue = 0;
                }         
                   
                double a = intValue - expectedCount;
                chisq += (a*a)/expectedCount;
            }
            return chisq;
        }

        /// <summary>
        /// Chi squared P of samples.
        /// </summary>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="sampleSize">Size of the sample.</param>
        /// <param name="histogram">The histogram of samples</param>
        /// <exception cref="ArgumentNullException">Throws when the <paramref name="histogram"/> value is null.</exception>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="maxValue"/> argument is less than or equal to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="sampleSize"/> argument is less than or equal to 0.</exception>
        [SuppressMessage("ReSharper", "RedundantCast")]
        public static double ChiSquaredP(int maxValue, int sampleSize, IDictionary<int, int> histogram)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException(nameof(histogram));
            }

            if (maxValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue),"Must be greater than 0.");
            }

            if (sampleSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSize), "Must be greater than 0.");
            }

            double expectedCount = (double)sampleSize / (double)(maxValue);

#if PORTABLE
            double chisq = ChiSquaredP(new Tuple<int, int>(0,maxValue), histogram, expectedCount);

            return chisq;
#else
            var partitioner = Partitioner.Create(0, maxValue);

            ConcurrentBag<double> sums = new ConcurrentBag<double>();
            Parallel.ForEach(partitioner, range =>
            {
                double partialChisq = ChiSquaredP(range, histogram, expectedCount);
                sums.Add(partialChisq);
            });

            return sums.Sum();
#endif
        }

        private static double ChiSquaredP(in Tuple<int, int> range, in IDictionary<int, int> histogram, in double expectedCount)
        {
            double partialChisq = 0;
            for (int i = range.Item1; i < range.Item2; i++)
            {
                if (histogram.TryGetValue(i, out var intValue) == false)
                {
                    intValue = 0;
                }                    
                double a = intValue - expectedCount;
                partialChisq += (a*a)/expectedCount;
            }
            return partialChisq;
        }
    }
}