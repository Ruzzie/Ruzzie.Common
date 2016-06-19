using System;
using Ruzzie.Common.Collections;
using Ruzzie.Common.Numerics;

namespace Ruzzie.Common
{
    /// <summary>
    /// Simple thread safe pseudo random number generator.
    /// </summary>
    public class SimpleRandom : Random
    {
        private readonly RandomSampler _randomSampler;
        private const int IntMaxValueMinusOne = int.MaxValue - 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRandom"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="hValue">The h value.</param>
        /// <param name="eValue">The e value.</param>
        public SimpleRandom(int seed, int hValue, int eValue)
        {
            //For bytes: 0,00106736330262713 127,5 H1741966517 E1631200041 
            //0,000000001594884 0,499999998405116 H1612099793 E1610967361, with _pTwo PrimeToolHash.GetPrime(hashOrValue.FindNearestPowerOfTwoLessThan())
            _randomSampler = new RandomSampler(seed, hValue, eValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRandom"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public SimpleRandom(int seed = 1) : this(seed, 952993412, 46847810)
        {

        }
       

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.
        /// </returns>
        public override int Next()
        {
            return _randomSampler.Next(int.MaxValue);
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public override double NextDouble()
        {
            int next = Next();
            double random = next / (double)(IntMaxValueMinusOne);
            return random;
        }

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        protected override double Sample()
        {
            return NextDouble();
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue"> The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue is greater than maxValue.");
            }
            double random = NextDouble();
            int range = maxValue - minValue;

            return (int)(minValue + Math.Floor(random * range));
        }

        /// <summary>
        ///Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="System.ArgumentNullException">buffer is null.</exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = NextByte();
            }
        }

        /// <summary>
        /// Returns a random byte.
        /// </summary>
        /// <returns>A random byte.</returns>
        public byte NextByte()
        {
            return (byte)(Next() & (byte.MaxValue));
        }

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">exclusiveMaximum is less than zero.</exception>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue. However, if maxValue equals 0, maxValue is returned.</returns>
        public override int Next(int maxValue)
        {
            if (maxValue == 0)
            {
                return 0;
            }

            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "is less than zero");
            }

            return _randomSampler.Next(maxValue);
        }

        internal class RandomSampler
        {
            private readonly ulong _moduloValue;
            private readonly ulong _pOnePowThree;
            private readonly ulong _pTwoPowTwo;
            private ConcurrentCircularOverwriteBuffer<ulong> _buffer;

            public RandomSampler(int seed, int hValue, int eValue)
            {
                int noiseVariable;

                unchecked
                {
                    noiseVariable = eValue ^ hValue;
                }

                _moduloValue = (ulong)(int.MaxValue + Convert.ToInt64(noiseVariable)).GetPrime();
                ulong pOne = (ulong)PrimeHelper.GetPrime(noiseVariable);
                ulong pTwo = (ulong)hValue;

                unchecked
                {
                    _pOnePowThree = (pOne * pOne * pOne);
                    _pTwoPowTwo = pTwo * pTwo;
                }

                InitializeNewBuffer(seed);
            }

            private void InitializeNewBuffer(int seed)
            {
                _buffer = new ConcurrentCircularOverwriteBuffer<ulong>(Environment.ProcessorCount*4);
                GenerateSampleFromSeed(seed);
            }

            private void GenerateSampleFromSeed(int seed)
            {
                ulong sample = Sample((ulong)seed);
                _buffer.WriteNext(sample);
            }

            public int Next(int exclusiveMaximum)
            {
                unchecked
                {
                    ulong number = NextSample();
                    return (int)(number % (ulong)exclusiveMaximum);
                }
            }

            private ulong NextSample()
            {
                ulong number;
                while (!_buffer.ReadNext(out number))
                {
                }

                _buffer.WriteNext(Sample(number));
                return number;
            }

            private ulong Sample(ulong currentSeed)
            {
                unchecked
                {
                    return ((currentSeed * _pOnePowThree) - (_pTwoPowTwo)) % _moduloValue;
                }
            }

            public void Reset(int newSeed)
            {
               InitializeNewBuffer(newSeed);
            }
        }

        /// <summary>
        /// Resets the the generator with a new seed.
        /// </summary>
        /// <param name="newSeed">The new seed.</param>
        public void Reset(int newSeed)
        {
            _randomSampler.Reset(newSeed);
        }
    }
}
