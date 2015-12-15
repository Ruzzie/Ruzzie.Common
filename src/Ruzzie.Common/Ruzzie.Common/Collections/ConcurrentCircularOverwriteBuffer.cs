using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Ruzzie.Common.Numerics;
//since volatile is used with interlocking, disable the warning.
#pragma warning disable 420

namespace Ruzzie.Caching.Collections
{
    /// <summary>
    ///     Circular buffer that overwrites values when the capacity is reached.
    ///     This buffer is threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of the values to buffer.</typeparam>
    public class ConcurrentCircularOverwriteBuffer<T>
    {
        private const int DefaultBufferSize = 1024;
        private readonly int _capacity;
        private volatile T[] _buffer;
        private volatile int _writeHeader;
        private volatile int _readHeader;
        private volatile int _count;
        private readonly int _indexMask;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCircularOverwriteBuffer{T}" /> class. With default buffer size of <see cref="DefaultBufferSize"/>.
        /// </summary>      
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue",Justification = "Required for CA1026")]
        public ConcurrentCircularOverwriteBuffer():this(DefaultBufferSize)
        {
            
        }
               
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCircularOverwriteBuffer{T}" /> class.
        /// </summary>
        /// <param name="capacity">The desired size. Internally this will always be set to a power of 2 for performance. Default is <see cref="DefaultBufferSize"/></param>
        /// <exception cref="ArgumentOutOfRangeException">size;Size has to be greater or equal to 2.</exception>
        public ConcurrentCircularOverwriteBuffer(int capacity = DefaultBufferSize)
        {
            if (capacity < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Size has to be greater or equal to 2.");
            }
            _capacity = capacity.FindNearestPowerOfTwoEqualOrGreaterThan();
            _buffer = new T[_capacity];
            _indexMask = _capacity - 1;

            _writeHeader = _indexMask;
            _readHeader = _indexMask;
        }

        /// <summary>
        /// Returns the number of values in the buffer.
        /// </summary>
        /// <value>
        /// The current item count of the buffer.
        /// </value>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Copies current buffer to target array starting at the specified destination array index. 
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the current buffer.</param>
        /// <param name="index">A 32-bit integer that represents the index in <paramref name="array"/> at which copying begins.</param>
        /// <remarks>This method uses the <see cref="Array.CopyTo(Array,int)"/> method to copy the current buffer to destination array. (this is a shallow copy)</remarks>   
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than the lower bound of
        /// <paramref name="array" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="array" /> is multidimensional.-or-The number of elements in the
        /// source array is greater than the available number of elements from <paramref name="index" /> to the end of the
        /// destination <paramref name="array" />.</exception>
        /// <exception cref="ArrayTypeMismatchException">The type of the source <see cref="T:System.Array" /> cannot be cast
        /// automatically to the type of the destination <paramref name="array" />.</exception>
        /// <exception cref="RankException">The source array is multidimensional.</exception>
        /// <exception cref="InvalidCastException">At least one element in the source <see cref="T:System.Array" /> cannot be cast
        /// to the type of destination <paramref name="array" />.</exception>
        public void CopyTo(Array array, int index)
        {
            _buffer.CopyTo(array, index);
        }

        /// <summary>
        /// Writes a value to the buffer.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteNext(T value)
        {
            int writeHeaderLocal = _writeHeader;

            while (Interlocked.CompareExchange(ref _writeHeader, NextIndex(writeHeaderLocal), writeHeaderLocal) != writeHeaderLocal)
            {
                SpinWait();
                writeHeaderLocal = _writeHeader;
            }

            _buffer[writeHeaderLocal] = value;

            if (_count < _capacity)
            {
                Interlocked.Increment(ref _count);
            }
        }

        /// <summary>
        /// Reads the next value from the buffer.
        /// </summary>
        /// <returns>The value read. if no value is present an <see cref="InvalidOperationException"/> will be thrown.</returns>
        /// <exception cref="System.InvalidOperationException">Error there is no next value.</exception>
        /// <exception cref="InvalidOperationException">There is no next value.</exception>
        public T ReadNext()
        {
            T value;
            if (ReadNext(out value))
            {
                return value;
            }
            throw new InvalidOperationException("Error there is no next value.");
        }

        /// <summary>
        /// Reads the next value from the buffer. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if a value could be read. If no next value is present false will be returned.</returns>
        public bool ReadNext(out T value)
        {
            if (_count == 0)
            {
                value = default(T);
                return false;
            }

            int readHeaderLocal = _readHeader;

            while (Interlocked.CompareExchange(ref _readHeader, NextIndex(readHeaderLocal), readHeaderLocal) != readHeaderLocal)
            {
                SpinWait();
                readHeaderLocal = _readHeader;
            }

            value = _buffer[readHeaderLocal];

            Interlocked.Decrement(ref _count);

            return true;
        }

        private int NextIndex(int currentHeader)
        {
            return (currentHeader + 1) & (_indexMask);
        }

#if PORTABLE
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static void SpinWait()
        {
            int op = 4;
            op = op << 1;
        }
#else
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")] private static readonly int SpinWaitIterations = 1;
        private static void SpinWait()
        {
            Thread.SpinWait(SpinWaitIterations);
        }
#endif
    }
}