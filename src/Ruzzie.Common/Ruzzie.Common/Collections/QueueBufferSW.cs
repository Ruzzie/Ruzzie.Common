using System.Buffers;
using System.Runtime.CompilerServices;
using Ruzzie.Common.Threading;

namespace Ruzzie.Common.Collections;

/// <summary>
/// A Buffer (queue) that supports
/// multiple concurrent (lock-free, not wait free (spinning)) writers  (MPSC)
/// ,single reader, that reads in batches
///   the reader should only read every N messages, otherwise another
///   data-structure is more appropriate.
///
/// This data-structure is optimized for multiple-write, single batch reader and
///   trades it for increased memory usage (capacity * 2).
/// </summary>
///
/// In a scenario where there a multiple produces that produce messages that
///   need be consumed by a single consumer, this could be an efficient
///   data-structure to use.
///
/// <typeparam name="T"></typeparam>
/// <remarks>
///
/// A double buffer (front / back ) is used to separate the reading from the
///   writing this is optimized for lot's of single concurrent fast writes and
///   a single Batch Read every xxx writes.
///
///  This implementation uses atomic operations and spinning as a synchronization mechanism.
/// </remarks>
public sealed class QueueBufferSW<T> : IQueueBuffer<T>
{
    private readonly ArrayPool<T>? _arrayPool;
    private readonly T[][]         _doubleBuffers;
    private readonly ulong         _capacity;

    /// contains in which buffer to write (front or back) and the nat. index of the buffer
    //   note: when we use a ulong value directly and the appropriate Interlocked methods the ReadBuffer does not
    //         function correctly; (my guess is it has something to do with cache-line, .net core and u-longs)
    //         although the logic is exactly the same.
    //         So now we use the VolatileLong wrapper (which uses a long (signed)) under the hood and everything is ok!
    private VolatileLong _writeHeader;

    private readonly AtomicBool _lockedForReading;

    /// <summary>
    /// Creates a new <see cref="QueueBufferSL{T}"/>
    /// </summary>
    /// <param name="capacity">the fixed max. capacity of the buffer</param>
    /// <param name="arrayPool">optional <see cref="ArrayPool{T}"/> to allocate the arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="capacity"/> is 0 or less</exception>
    public QueueBufferSW(int capacity = 1024, ArrayPool<T>? arrayPool = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too small");
        }

        if (capacity > (int)QueueBufferSW.SELECT_INDEX_MASK)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too large");
        }

        _arrayPool     = arrayPool;
        _doubleBuffers = new T[2][];

        _doubleBuffers[0] = arrayPool?.Rent(capacity) ?? new T[capacity];
        _doubleBuffers[1] = arrayPool?.Rent(capacity) ?? new T[capacity];

        _capacity = (ulong)capacity;

        _lockedForReading = new AtomicBool(false);
        _writeHeader      = 0;
    }

    internal struct DoubleBuffer
    {
        private readonly T[] _a;
        private readonly T[] _b;

        public DoubleBuffer(T[] a, T[] b)
        {
            _a = a;
            _b = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetBuffer(BufferIndex bufferIndex)
        {
            return bufferIndex == BufferIndex.A ? _a : _b;
        }

        public enum BufferIndex
        {
            A = 0
          , B = 1
        }
    }


    /// Add an item to the queue, this is thread-safe
    [SkipLocalsInit]
    public bool TryAdd(in T value)
    {
        if (QueueBufferSW.SelectIndex((ulong)_writeHeader.ReadUnfenced()) >= _capacity)
        {
            // fast path full check
            return false;
        }

        var loopCount = 0;


        //1. INCREMENT THE PRODUCER COUNT FOR THE CURRENT WRITE BUFFER
        var spinWait = new SpinWait();

        ulong nextHeader;
        ulong currentHeader;

        do
        {
            if (++loopCount > 0)
            {
                spinWait.SpinOnce();
            }

            currentHeader = (ulong)_writeHeader.VolatileValue;

            // GET WRITE SLOT (IDX)
            //  since the idx are the LSB bits, we can just increment -----v
            nextHeader = QueueBufferSW.IncrementProducer(currentHeader) + 1;

            if (QueueBufferSW.SelectIndex(nextHeader) > _capacity)
            {
                return false;
            }
        } // ATOMIC START PRODUCING
        while (!_writeHeader.AtomicCompareExchange((long)nextHeader, (long)currentHeader));

        //2. WRITE TO THE FRONT BUFFER
        try
        {
            // 0 or 1, depending on the front or back-buffer
            var bufferIdx    = QueueBufferSW.SelectCurrentBufferIdx(nextHeader);
            var nextWriteIdx = QueueBufferSW.SelectIndex(nextHeader);

            if (nextWriteIdx > _capacity)
            {
                // full
                return false;
            }

            _doubleBuffers[bufferIdx][nextWriteIdx - 1] = value;
        }
        finally
        {
            //3. DECREMENT THE PRODUCER COUNT
            //  Done producing so decrementProducer
            ulong updatedHeader;
            do
            {
                currentHeader = (ulong)_writeHeader.VolatileValue;
                updatedHeader = QueueBufferSW.DecrementProducer(currentHeader);
            } while (!_writeHeader.AtomicCompareExchange((long)updatedHeader, (long)currentHeader));
        }

        return true;
    }


    /// Snapshots the writes up until now and returns a ReadHandle from which
    ///   you can read all the items.
    /// The ReadHandle should be disposed after use to free the ReadHandle.
    /// Supports only one ReadHandle at a time.
    /// <remarks>
    /// Effectively this methods swaps the front and back buffer to snapshot
    ///   (and let writers continue writing).
    /// </remarks>
    [SkipLocalsInit]
    public IQueueBuffer<T>.ReadHandle ReadBuffer()
    {
        if (_lockedForReading.ReadCompilerFenced())
        {
            // no swap, the lock is not freed by the reader yet,
            //   a ReadHandle is still in use
            throw new
                InvalidOperationException("Tried to create a ReadHandle for the Buffer, but another ReadHandle was already given. Maybe the previous ReadHandle was not freed yet?");
        }

        _lockedForReading.WriteUnfenced(true);
        // we need to wait until there are no more producers producing in the current write buffer
        //   then we swap the buffers

        ulong currentHeader;
        ulong doneProducingHeader;
        ulong nextHeader;

        do
        {
            currentHeader = (ulong)_writeHeader.VolatileValue;
            /*var readAttempts = 0;
        READ_HEADER:

            currentHeader = (ulong)_writeHeader.CompilerFencedValue;

            // Adding the extra check on producer count and the SpinWait fixes the behavior
            //   This is not what I expected, since the compare exchange with the producers set to zero as
            //   comparison in the while loop should do the same (at least in my head).
            var currentProducersCount = QueueBufferSW.SelectCurrentProducerCount(currentHeader);
            if (currentProducersCount > 0)
            {
                // this effectively spins until a point is reached where there is no producer active
                //  note: when the contention is extremely high, this performs reasonably poor and a more constant perf
                //        of the default QueueBuffer is recommended (more consistent performance)
                if (++readAttempts > 1000)
                {
                    throw new Exception("More than 1000 attempts waiting on gap in producer");
                }

                goto READ_HEADER;
            }*/

            // The header with all producers set to 0, that is the condition we are waiting for.
            doneProducingHeader = currentHeader & QueueBufferSW.CLEAR_PRODUCERS_MASK;

            // swapped buffer
            nextHeader = QueueBufferSW.SwapAndResetWriteIndex(doneProducingHeader);
        } while (!_writeHeader.AtomicCompareExchange((long)nextHeader, (long)doneProducingHeader));


        // so now we have swapped the buffers and we know how many items are in the 'backbuffer'

        // due to optimization (and concurrency) the _index could be larger than the
        //   length so we need to take that into account when calculating the length
        var numberOfItems = Math.Min(QueueBufferSW.SelectIndex(currentHeader), _capacity);
        var bufferIdx     = QueueBufferSW.SelectCurrentBufferIdx(currentHeader);

        var data = new ReadOnlySpan<T>(_doubleBuffers[bufferIdx]
                                     , 0
                                     , (int)numberOfItems);

        return new IQueueBuffer<T>.ReadHandle(data, _lockedForReading);
    }

    /// Returns the backing arrays to the array pool if they were allocated
    public void Dispose()
    {
        try
        {
            _arrayPool?.Return(_doubleBuffers[0]);
        }
        finally
        {
            _arrayPool?.Return(_doubleBuffers[1]);
        }
    }
}

[SkipLocalsInit]
internal static class QueueBufferSW
{
    //                                current_producer_count
    //                                 buffer
    //                                    |
    //         write buffer select --+    |         writeIndex in buffer (23 bits) (8_388_607)
    //                               |    |               |
    //                               v /------\ /---------------------\
    internal const ulong AT_WR_HD = 0b0_00000000_00000000000000000000000;
    //                               \---------------------------------/
    //                                       ATOMIC WRITE HEADER
    //                        MP SC Array backed double-buffer implementation
    //

    internal const ulong SELECT_BUFFER_MASK = 0b1_00000000_00000000000000000000000;
    internal const ulong SELECT_INDEX_MASK  = 0b0_00000000_11111111111111111111111;

    private const int PRODUCER_COUNT_SIZE_IN_BITS = 8;
    private const int WRITE_INDEX_SIZE_IN_BITS    = 23;

    private const  ulong PRODUCER_COUNT_MASK  = (1 << PRODUCER_COUNT_SIZE_IN_BITS) - 1;
    internal const ulong CLEAR_PRODUCERS_MASK = SELECT_BUFFER_MASK | SELECT_INDEX_MASK;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte SelectCurrentBufferIdx(ulong packedBits)
    {
        return (byte)((packedBits & SELECT_BUFFER_MASK) >> 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SelectCurrentProducerCount(ulong packedBits)
    {
        return ((PRODUCER_COUNT_MASK << WRITE_INDEX_SIZE_IN_BITS) & packedBits) >> WRITE_INDEX_SIZE_IN_BITS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SetProducerCount(ulong packedBits, ulong producerCount)
    {
        return (~(PRODUCER_COUNT_MASK << WRITE_INDEX_SIZE_IN_BITS) & packedBits) |
               ((producerCount) << WRITE_INDEX_SIZE_IN_BITS);
    }

    /// Increments the producer count
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong IncrementProducer(ulong packedBits)
    {
        //TODO: BOUNDS CHECK
        var currentCount = SelectCurrentProducerCount(packedBits);

        return SetProducerCount(packedBits, currentCount + 1);
    }

    /// Decrements the producer count
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong DecrementProducer(ulong packedBits)
    {
        var currentCount = SelectCurrentProducerCount(packedBits);

        return SetProducerCount(packedBits, currentCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SwapAndResetWriteIndex(ulong packedBits)
    {
        return (packedBits ^ SELECT_BUFFER_MASK) & (SELECT_BUFFER_MASK | ~SELECT_INDEX_MASK);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SelectIndex(ulong packedBits)
    {
        return packedBits & SELECT_INDEX_MASK;
    }
}