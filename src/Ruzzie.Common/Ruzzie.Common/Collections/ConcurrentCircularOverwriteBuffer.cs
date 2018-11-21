using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ruzzie.Common.Numerics;
using Ruzzie.Common.Threading;

//since volatile is used with interlocking, disable the warning.
#pragma warning disable 420
namespace Ruzzie.Common.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentCircularOverwriteBuffer<T>
    {
        /// <summary>
        ///     Copies current buffer to target array starting at the specified destination array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the current buffer.</param>
        /// <param name="index">A 32-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
        /// <remarks>
        ///     This method uses the <see cref="Array.CopyTo(Array,int)" /> method to copy the current buffer to destination
        ///     array. (this is a shallow copy)
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than the lower bound of
        ///     <paramref name="array" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="array" /> is multidimensional.-or-The number of elements in the
        ///     source array is greater than the available number of elements from <paramref name="index" /> to the end of the
        ///     destination <paramref name="array" />.
        /// </exception>
        /// <exception cref="ArrayTypeMismatchException">
        ///     The type of the source <see cref="T:System.Array" /> cannot be cast
        ///     automatically to the type of the destination <paramref name="array" />.
        /// </exception>
        /// <exception cref="RankException">The source array is multidimensional.</exception>
        /// <exception cref="InvalidCastException">
        ///     At least one element in the source <see cref="T:System.Array" /> cannot be cast
        ///     to the type of destination <paramref name="array" />.
        /// </exception>
        void CopyTo(in Array array, in int index);

        /// <summary>
        ///     Writes a value to the buffer.
        /// </summary>
        /// <param name="value">The value to write</param>
        void WriteNext(in T value);

        /// <summary>
        ///     Reads the next value from the buffer.
        /// </summary>
        /// <returns>The value read. if no value is present an <see cref="InvalidOperationException" /> will be thrown.</returns>
        /// <exception cref="System.InvalidOperationException">Error there is no next value.</exception>
        /// <exception cref="InvalidOperationException">There is no next value.</exception>
        T ReadNext();

        /// <summary>
        ///     Reads the next value from the buffer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if a value could be read. If no next value is present false will be returned.</returns>
        bool ReadNext(out T value);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Circular buffer that overwrites values when the capacity is reached.
    ///     This buffer is threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of the values to buffer.</typeparam>
    public class ConcurrentCircularOverwriteBuffer<T> : IConcurrentCircularOverwriteBuffer<T>
    {
        private const int DefaultBufferSize = 1024;
        private readonly int _capacity;
        private readonly T[] _buffer;
        private readonly long _indexMask;
        private VolatileLong _writeHeader;
        private VolatileLong _readHeader;

        /// <summary>
        ///     Returns the number of values in the buffer.
        /// </summary>
        /// <value>
        ///     The current item count of the buffer.
        /// </value>
        public long Count
        {
            get
            {
                unchecked
                {
                    long itemCount = (_writeHeader.CompilerFencedValue + 1) - (_readHeader.CompilerFencedValue + 1);

                    if (itemCount == 0)
                    {
                        return 0;
                    }
                    long remainder = (itemCount%_capacity);

                    if (remainder > 0)
                    {
                        return itemCount/_capacity + remainder;
                    }
                    return _capacity;
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Ruzzie.Common.Collections.ConcurrentCircularOverwriteBuffer`1" /> class. With default buffer
        ///     size of <see cref="F:Ruzzie.Common.Collections.ConcurrentCircularOverwriteBuffer`1.DefaultBufferSize" />.
        /// </summary>
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue", Justification = "Required for CA1026")]
        public ConcurrentCircularOverwriteBuffer() : this(DefaultBufferSize)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCircularOverwriteBuffer{T}" /> class.
        /// </summary>
        /// <param name="capacity">
        ///     The desired size. Internally this will always be set to a power of 2 for performance. Default is
        ///     <see cref="DefaultBufferSize" />
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">size;Size has to be greater or equal to 2.</exception>
        public ConcurrentCircularOverwriteBuffer(in int capacity = DefaultBufferSize)
        {
            if (capacity < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Size has to be greater or equal to 2.");
            }
            _capacity = capacity.FindNearestPowerOfTwoEqualOrGreaterThan();
            _buffer = new T[_capacity];
            _indexMask = _capacity - 1;

            _writeHeader = -1;
            _readHeader = -1;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Copies current buffer to target array starting at the specified destination array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the current buffer.</param>
        /// <param name="index">A 32-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
        /// <remarks>
        ///     This method uses the <see cref="M:System.Array.CopyTo(System.Array,System.Int32)" /> method to copy the current buffer to destination
        ///     array. (this is a shallow copy)
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than the lower bound of
        ///     <paramref name="array" />.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="array" /> is multidimensional.-or-The number of elements in the
        ///     source array is greater than the available number of elements from <paramref name="index" /> to the end of the
        ///     destination <paramref name="array" />.
        /// </exception>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        ///     The type of the source <see cref="T:System.Array" /> cannot be cast
        ///     automatically to the type of the destination <paramref name="array" />.
        /// </exception>
        /// <exception cref="T:System.RankException">The source array is multidimensional.</exception>
        /// <exception cref="T:System.InvalidCastException">
        ///     At least one element in the source <see cref="T:System.Array" /> cannot be cast
        ///     to the type of destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(in Array array, in int index)
        {
            _buffer.CopyTo(array, index);
        }

        /// <summary>
        ///     Writes a value to the buffer.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteNext(in T value)
        {
            long nextWriteIndex = _writeHeader.AtomicIncrement();
            long currentWriteIndex = nextWriteIndex - 1;

            _buffer[currentWriteIndex & _indexMask] = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Reads the next value from the buffer.
        /// </summary>
        /// <returns>The value read. if no value is present an <see cref="T:System.InvalidOperationException" /> will be thrown.</returns>
        /// <exception cref="T:System.InvalidOperationException">Error there is no next value.</exception>
        /// <exception cref="T:System.InvalidOperationException">There is no next value.</exception>
        public T ReadNext()
        {
            if (ReadNext(out var value))
            {
                return value;
            }
            throw new InvalidOperationException("Error there is no next value.");
        }

        /// <inheritdoc />
        /// <summary>
        ///     Reads the next value from the buffer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if a value could be read. If no next value is present false will be returned.</returns>
        public bool ReadNext(out T value)
        {
            if (!HasNext(_readHeader.ReadUnfenced(), _writeHeader.ReadUnfenced()))
            {
                value = default;
                return false;
            }

            long nextReadIndex = _readHeader.AtomicIncrement();
            long currentReadIndex = nextReadIndex - 1;

            value = _buffer[currentReadIndex & _indexMask];

            return true;
        }

#if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool HasNext(in long readHeader, in long writeHeader)
        {
            return writeHeader - readHeader != 0;           
        }
    }
}