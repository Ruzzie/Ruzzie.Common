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
public sealed class QueueBuffer<T> : IDisposable
{
    private readonly ArrayPool<T>? _arrayPool;

    private readonly T[][] _doubleBuffers;

    /// <summary>
    /// Creates a new <see cref="QueueBuffer{T}"/>
    /// </summary>
    /// <param name="capacity">the minimum capacity</param>
    /// <param name="arrayPool">optional <see cref="ArrayPool{T}"/> to allocate the arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="capacity"/> is 0 or less</exception>
    public QueueBuffer(int capacity = 1024, ArrayPool<T>? arrayPool = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too small");
        }

        if (capacity >= QueueBuffer.BufferSelectionMask)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "too large");
        }

        _arrayPool     = arrayPool;
        _doubleBuffers = new T[2][];

        _doubleBuffers[0] = arrayPool?.Rent(capacity) ?? new T[capacity];
        _doubleBuffers[1] = arrayPool?.Rent(capacity) ?? new T[capacity];

        _capacity = capacity;
    }

    private readonly int _capacity;

    private int _writeHeader; // contains in which buffer to write (front or back) and the nat. index of the buffer

    private  SpinLock _spinLock        = new SpinLock();
    internal bool     LockedForReading = false;

    /// Add an item to the queue, this is thread-safe
    public bool TryAdd(in T value)
    {
        // fast path
        if (QueueBuffer.SelectIndex(_writeHeader) >= _capacity)
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
            var nextWriteHeader =
                // since we use a spinLock, can we omit atomic operations ...?
                ++_writeHeader - 1;
            //Interlocked.Increment(ref _writeHeader) - 1;

            // remove the the buffer selection mask
            //  such that we can index the array by its natural index
            var nextIndex = QueueBuffer.SelectIndex(nextWriteHeader);

            if (nextIndex >= _capacity)
            {
                //full
                return false;
            }

            // write to the front buffer
            // fun exercise, is it branch free when we use a T[][] of 2 and use an index ...?
            _doubleBuffers
                [
                 // 0 or 1, depending on the front or backbuffer
                 // select the current 'write' buffer
                 QueueBuffer.SelectBuffer(nextWriteHeader)][nextIndex] = value;
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
    public ReadHandle<T> ReadBuffer()
    {
        if (Volatile.Read(ref LockedForReading))
        {
            // no swap, the lock is not freed by the reader yet,
            //   a ReadHandle is still in use
            throw new
                InvalidOperationException("Tried to create a ReadHandle for the Buffer, but another ReadHandle was already given. Maybe the previous ReadHandle was not freed yet?");
        }

        /*if (Interlocked.Exchange(ref LockedForReading, true))
        {
            
        }*/

        var lockTaken = false;

        try
        {
            _spinLock.Enter(ref lockTaken);
            LockedForReading = true; // no volatile / atomic write needed since we lock

            var currentWriteHeader = _writeHeader;

            // the natural index can be reset to 0 and swap the front and back buffer
            var swappedWriteHeader = QueueBuffer.SwapAndResetWriteHeader(currentWriteHeader);


            //Compare and swap the write index (flip front and back buffer and reset write index)
            while (currentWriteHeader !=
                   Interlocked.CompareExchange(ref _writeHeader, swappedWriteHeader, currentWriteHeader))
            {
                currentWriteHeader = _writeHeader;
                swappedWriteHeader = QueueBuffer.SwapAndResetWriteHeader(currentWriteHeader);
            }

            // so now we have swapped the buffers and we know how many items are in the 'backbuffer'

            // due to optimization (and concurrency) the _index could be larger than the
            //   length so we need to take that into account when calculating the length
            var numberOfItems = Math.Min(QueueBuffer.SelectIndex(currentWriteHeader), _capacity);

            return new ReadHandle<T>(new ReadOnlySpan<T>(
                                                         _doubleBuffers[QueueBuffer.SelectBuffer(currentWriteHeader)]
                                                       , 0
                                                       , numberOfItems)
                                   , this);
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

/// A ReadHandle to read the Buffer, this should only be used in a single thread and should be immediately disposed after reading.
public readonly ref struct ReadHandle<T> // : IDisposable
{
    private readonly ReadOnlySpan<T> _backBuffer;
    private readonly QueueBuffer<T>  _owner;

    internal ReadHandle(ReadOnlySpan<T> backBuffer, QueueBuffer<T> owner)
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

internal static class QueueBuffer
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