using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ruzzie.Common.Threading
{
    /// <summary>
    /// A structure to capture a long value on a single cache line. With different read and write strategies for usage across threads.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2)]
    public struct VolatileLong
    {
        internal const int CacheLineSize = 64;

        [FieldOffset(CacheLineSize)]
        private long _value;

        private VolatileLong(long value)
        {
            _value = value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="long"/> to <see cref="VolatileLong"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator VolatileLong(in long value)
        {
            return new VolatileLong(value);
        }

        /// <summary>
        /// Reads or writes the value with full memorybarrier, this inserts a memory barrier that prevents the processor from reordering memory operations.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public long VolatileValue
        {
            get
            {
                return Volatile.ReadValueType(ref _value);

            }
            set
            {
                Volatile.WriteValueType(ref _value, value);
            }
        }

        /// <summary>
        /// Reads or writes the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public long CompilerFencedValue
        {
            [MethodImpl(MethodImplOptions.NoOptimization)] get { return _value; }
            [MethodImpl(MethodImplOptions.NoOptimization)] set { _value = value; }
        }

        /// <summary>
        /// Reads the unfenced value.
        /// </summary>
        /// <returns>the value</returns>
        public long ReadUnfenced()
        {
            return _value;
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <returns>true if the exchange the comparand was equal to the current value, otherwise false.</returns>
        public bool AtomicCompareExchange(in long newValue, in long comparand)
        {
            return Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand;
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <returns>The incremented value.</returns>
        public long AtomicIncrement()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically decrement the current value and return the new value
        /// </summary>
        /// <returns>The decremented value.</returns>
        public long AtomicDecrement()
        {
            return Interlocked.Decrement(ref _value);
        }
    }
}