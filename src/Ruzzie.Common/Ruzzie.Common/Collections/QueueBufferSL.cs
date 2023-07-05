using System.Buffers;
using System.Runtime.CompilerServices;
using Volatile = System.Threading.Volatile;

namespace Ruzzie.Common.Collections;

/// <summary>
/// A Buffer (queue) that supports
/// multiple concurrent writers  (MPSC)
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
/// This implementation uses a SpinLock as a 'locking' mechanism.
/// </remarks>
public sealed class QueueBufferSL<T> : IQueueBuffer<T>
{
    private readonly ArrayPool<T>? _arrayPool;
    private readonly T[][]         _doubleBuffers;
    private readonly int           _capacity;

    /// contains in which buffer to write (front or back) and the nat. index of the buffer
    private int _writeHeader;

    private          SpinLock  _spinLock         = new SpinLock();
    private readonly Ref<bool> _lockedForReading = new Ref<bool>(false);

    /// <summary>
    /// Creates a new <see cref="QueueBufferSL{T}"/>
    /// </summary>
    /// <param name="capacity">the fixed max. capacity of the buffer</param>
    /// <param name="arrayPool">optional <see cref="ArrayPool{T}"/> to allocate the arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="capacity"/> is 0 or less</exception>
    public QueueBufferSL(int capacity = 1024, ArrayPool<T>? arrayPool = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too small");
        }

        if (capacity >= QueueBufferSL.BufferSelectionMask)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too large");
        }

        _arrayPool     = arrayPool;
        _doubleBuffers = new T[2][];

        _doubleBuffers[0] = arrayPool?.Rent(capacity) ?? new T[capacity];
        _doubleBuffers[1] = arrayPool?.Rent(capacity) ?? new T[capacity];

        _capacity = capacity;
    }


    /// Add an item to the queue, this is thread-safe
    [SkipLocalsInit]
    public bool TryAdd(in T value)
    {
        // fast path
        if (QueueBufferSL.SelectIndex(_writeHeader) >= _capacity)
        {
            //full
            return false;
        }

        bool lockTaken = false;
        try
        {
            //Todo: error handling strategy, this stuff can throw exceptions all around
            _spinLock.Enter(ref lockTaken);

            // since we increment first
            //   we subtract 1. When we start at 0
            //   the increment will set it to 1.
            //   however we need write in Index 0;
            // since we use a spinLock, we omit atomic operations
            var nextWriteHeader = ++_writeHeader - 1;

            // remove the the buffer selection mask
            //  such that we can index the array by its natural index
            var nextIndex = QueueBufferSL.SelectIndex(nextWriteHeader);

            if (nextIndex >= _capacity)
            {
                //full
                return false;
            }

            // write to the front buffer
            _doubleBuffers
                [
                 // 0 or 1, depending on the front or backbuffer
                 // select the current 'write' buffer
                 QueueBufferSL.SelectBuffer(nextWriteHeader)][nextIndex] = value;
        }
        finally
        {
            if (lockTaken)
                _spinLock.Exit();
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
    public ReadHandle<T> ReadBuffer()
    {
        if (Volatile.Read(ref _lockedForReading.Value))
        {
            // no swap, the lock is not freed by the reader yet,
            //   a ReadHandle is still in use
            throw new
                InvalidOperationException("Tried to create a ReadHandle for the Buffer, but another ReadHandle was already given. Maybe the previous ReadHandle was not freed yet?");
        }

        var lockTaken = false;

        try
        {
            _spinLock.Enter(ref lockTaken);
            _lockedForReading.Value = true; // no volatile / atomic write needed since we lock

            var currentWriteHeader = _writeHeader;

            // the natural index can be reset to 0 and swap the front and back buffer
            var swappedWriteHeader = QueueBufferSL.SwapAndResetWriteHeader(currentWriteHeader);


            //Compare and swap the write index (flip front and back buffer and reset write index)
            while (currentWriteHeader !=
                   Interlocked.CompareExchange(ref _writeHeader, swappedWriteHeader, currentWriteHeader))
            {
                currentWriteHeader = _writeHeader;
                swappedWriteHeader = QueueBufferSL.SwapAndResetWriteHeader(currentWriteHeader);
            }

            // so now we have swapped the buffers and we know how many items are in the 'backbuffer'

            // due to optimization (and concurrency) the _index could be larger than the
            //   length so we need to take that into account when calculating the length
            var numberOfItems = Math.Min(QueueBufferSL.SelectIndex(currentWriteHeader), _capacity);

            var data = new ReadOnlySpan<T>(_doubleBuffers[QueueBufferSL.SelectBuffer(currentWriteHeader)]
                                         , 0
                                         , numberOfItems);


            return new ReadHandle<T>(data, _lockedForReading);
        }
        finally
        {
            if (lockTaken)
                _spinLock.Exit();
        }
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

internal static class QueueBufferSL
{
    // this indicates whether to write to the first or the second buffer (front or back)
    //  front and backbuffer are swapped by flipping this bit
    //   we should guard against the max size which is capped by this
    internal const int BufferSelectionMask = 0b1 << 30;
    internal const int SelectIndexMask     = 0b00111111_11111111_11111111_11111111;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SelectBuffer(int writeHeader)
    {
        return (writeHeader & BufferSelectionMask) >> 30;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SelectIndex(int writeHeader)
    {
        return writeHeader & SelectIndexMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SwapAndResetWriteHeader(int writeHeader)
    {
        // the natural index can be reset to 0 and swap the front and back buffer
        return (writeHeader ^ BufferSelectionMask) & ~SelectIndexMask;
    }
}