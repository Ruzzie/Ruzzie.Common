using System.Buffers;
using System.Runtime.CompilerServices;

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
/// </remarks>
public sealed class QueueBufferAlt<T> : IDisposable
{
    private readonly ArrayPool<T>? _arrayPool;
    private readonly T[][]         _doubleBuffers;
    private readonly ulong         _capacity;

    /// <summary>
    /// Creates a new <see cref="QueueBuffer{T}"/>
    /// </summary>
    /// <param name="capacity">the minimum capacity</param>
    /// <param name="arrayPool">optional <see cref="ArrayPool{T}"/> to allocate the arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="capacity"/> is 0 or less</exception>
    public QueueBufferAlt(int capacity = 1024, ArrayPool<T>? arrayPool = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too small");
        }

        if (capacity > (int)QueueBufferAlt.SELECT_INDEX_MASK)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too large");
        }

        _arrayPool     = arrayPool;
        _doubleBuffers = new T[2][];

        _doubleBuffers[0] = arrayPool?.Rent(capacity) ?? new T[capacity];
        _doubleBuffers[1] = arrayPool?.Rent(capacity) ?? new T[capacity];

        _capacity = (ulong)capacity;
    }


    private ulong _writeHeader; // contains in which buffer to write (front or back) and the nat. index of the buffer

    internal bool LockedForReading = false;


    /// Add an item to the queue, this is thread-safe
    public bool TryAdd(in T value)
    {
        if (QueueBufferAlt.SelectIndex(_writeHeader) >= _capacity)
        {
            // fast path full check
            return false;
        }

        try
        {
            // INCREMENT THE PRODUCER COUNT FOR THE CURRENT WRITE BUFFER 

            var nextHeader    = 0ul;
            var currentHeader = 0ul;
            var loopCount     = 0;
            var spinWait      = new SpinWait();

            do
            {
                if (++loopCount > 1)
                {
                    spinWait.SpinOnce();
                }

                currentHeader = Interlocked.Read(ref _writeHeader); // read the header, maybe do volatile read...?

                // GET WRITE SLOT (IDX)
                //  since the idx are the LSB bits, we can just increment -----v
                nextHeader = QueueBufferAlt.IncrementProducer(currentHeader) + 1;
            } // ATOMIC START PRODUCING 
            while (Interlocked.CompareExchange(ref _writeHeader, nextHeader, currentHeader) != currentHeader);


            // 0 or 1, depending on the front or backbuffer
            var bufferIdx    = QueueBufferAlt.SelectCurrentBufferIdx(nextHeader);
            var nextWriteIdx = QueueBufferAlt.SelectIndex(nextHeader);

            if (nextWriteIdx > _capacity)
            {
                // full
                return false;
            }

            // write to the front buffer
            _doubleBuffers[bufferIdx][nextWriteIdx - 1] = value;
        }
        finally
        {
            //Done producing decrementProducer
            var currentHeader = 0ul;
            var updatedHeader = 0ul;
            /*var loopCount     = 0;
            var spinWait      = new SpinWait();*/

            do
            {
                /*if (++loopCount > 1)
                {
                    spinWait.SpinOnce();
                }*/

                currentHeader = Volatile.Read(ref _writeHeader);
                //currentHeader = Interlocked.Read(ref _writeHeader);
                updatedHeader = QueueBufferAlt.DecrementProducer(currentHeader);
            } while (Interlocked.CompareExchange(ref _writeHeader, updatedHeader, currentHeader) != currentHeader);
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
    public ReadHandleAlt<T> ReadBuffer()
    {
        if (Volatile.Read(ref LockedForReading))
        {
            // no swap, the lock is not freed by the reader yet,
            //   a ReadHandle is still in use
            throw new
                InvalidOperationException("Tried to create a ReadHandle for the Buffer, but another ReadHandle was already given. Maybe the previous ReadHandle was not freed yet?");
        }

        Volatile.Write(ref LockedForReading, true);

        // we need to wait until there are no more producers producing in the current write buffer
        //   then we swap the buffers

        var currentHeader       = 0ul;
        var doneProducingHeader = 0ul;
        var nextHeader          = 0ul;

        do
        {
        READ_HEADER:
            currentHeader = Volatile.Read(ref _writeHeader);
            //currentHeader = Interlocked.Read(ref _writeHeader);
            //currentHeader = _writeHeader;

            // Adding the extra check on producer count and the SpinWait fixes the behavior
            //   This is not what I expected, since the compare exchange with the producers set to zero as 
            //   comparison in the while loop should do the same (at least in my head).
            var currentProducersCount = QueueBufferAlt.SelectCurrentProducerCount(currentHeader);
            if (currentProducersCount > 0)
            {
                goto READ_HEADER;
            }

            // The header with all producers set to 0, that is the condition we are waiting for.
            doneProducingHeader = currentHeader & QueueBufferAlt.CLEAR_PRODUCERS_MASK;

            // swapped buffer
            nextHeader = QueueBufferAlt.SwapAndResetWriteIndex(doneProducingHeader);
        } while (Interlocked.CompareExchange(ref _writeHeader, nextHeader, doneProducingHeader) != currentHeader);


        // so now we have swapped the buffers and we know how many items are in the 'backbuffer'

        // due to optimization (and concurrency) the _index could be larger than the
        //   length so we need to take that into account when calculating the length
        var numberOfItems = Math.Min(QueueBufferAlt.SelectIndex(doneProducingHeader), _capacity);


        var bufferIdx = QueueBufferAlt.SelectCurrentBufferIdx(doneProducingHeader);


        return new ReadHandleAlt<T>(new ReadOnlySpan<T>(_doubleBuffers[bufferIdx], 0, (int)numberOfItems), this);
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

/// A ReadHandle to read the Buffer, this should only be used in a single thread and should be immediately disposed after reading.
public readonly ref struct ReadHandleAlt<T> // : IDisposable
{
    private readonly ReadOnlySpan<T>   _backBuffer;
    private readonly QueueBufferAlt<T> _owner;

    internal ReadHandleAlt(ReadOnlySpan<T> backBuffer, QueueBufferAlt<T> owner)
    {
        if (owner.LockedForReading == false)
        {
            throw new
                InvalidOperationException("Given Buffer was not locked for reading. Cannot create a valid ReadHandle for this");
        }

        _backBuffer = backBuffer;
        _owner      = owner;
    }

    /// Get the contents of the buffer for reading
    public ReadOnlySpan<T> AsSpan()
    {
        if (_owner.LockedForReading == false)
        {
            throw new
                InvalidOperationException("The Buffer to read from was not locked for reading. This ReadHandle is invalid. Please dispose all ReadHandles after use, only one ReadHandle can be active at a time.");
        }

        return _backBuffer;
    }

    /// <summary>
    /// Disposes the handle
    /// </summary>
    public void Dispose()
    {
        // yuck....
        Volatile.Write(ref _owner.LockedForReading, false);
        // should we clear the array contents (for security and easier debugging purposes) ...?
    }
}

internal static class QueueBufferAlt
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
    //                        MP SC Array backed doublebuffer implementation
    //
    // TODO: use a ulong instead of ulong (x64 optimized?) 

    internal const ulong SELECT_BUFFER_MASK = 0b1_00000000_00000000000000000000000;
    internal const ulong SELECT_INDEX_MASK  = 0b0_00000000_11111111111111111111111;

    private const int PRODUCER_COUNT_SIZE_IN_BITS = 8;
    private const int WRITE_INDEX_SIZE_IN_BITS    = 23;

    private const ulong PRODUCER_COUNT_MASK  = (1 << PRODUCER_COUNT_SIZE_IN_BITS) - 1;
    public const  ulong CLEAR_PRODUCERS_MASK = SELECT_BUFFER_MASK | SELECT_INDEX_MASK;

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

    /// Increments the producer count for the current selected buffer
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong IncrementProducer(ulong packedBits)
    {
        var currentCount = SelectCurrentProducerCount(packedBits);

        return SetProducerCount(packedBits, currentCount + 1);
    }

    /// Decrements the producer count for the current selected buffer
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