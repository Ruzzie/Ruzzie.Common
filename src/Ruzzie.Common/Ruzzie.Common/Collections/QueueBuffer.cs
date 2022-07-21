using System.Buffers;

namespace Ruzzie.Common.Collections;


/// <summary>
/// A Buffer (queue) that supports
/// multiple concurrent (lock-free) writers (1 (max. 2?) cas op) (MPSC)
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

    /// multiple threads may write to the front-buffer
    private BufferSegment _frontBuffer;

    /// only one thread may read from the back-buffer
    private BufferSegment _backBuffer;

    // this is only used to ensure the single reader, reading is not lock-free
    //   however since there can only be a single consumer that reads in batches
    //   the contention should be low / non-existent
    private readonly object _freezeLock = new();

    /// <summary>
    /// Creates a new <see cref="QueueBuffer{T}"/>
    /// </summary>
    /// <param name="minCapacity">the minimum capacity</param>
    /// <param name="arrayPool">optional <see cref="ArrayPool{T}"/> to allocate the arrays from</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="minCapacity"/> is 0 or less</exception>
    public QueueBuffer(int minCapacity = 1024, ArrayPool<T>? arrayPool = null)
    {
        if (minCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minCapacity));
        }

        _arrayPool = arrayPool;

        _frontBuffer = new BufferSegment(arrayPool?.Rent(minCapacity) ?? new T[minCapacity]);
        _backBuffer  = new BufferSegment(arrayPool?.Rent(minCapacity) ?? new T[minCapacity]);
    }

    /// Add an item to the queue, this is thread-safe
    public bool TryAdd(in T value)
    {
        // always write to the front buffer
        // maybe we need a memory barrier, atomic read here, first we need
        //   some tests
        Interlocked.MemoryBarrier();
        return BufferSegment.TryAdd(_frontBuffer, value);
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
        lock (_freezeLock)
            // we take the hit of a mutex.
            //   since reading is done in batches the assumption
            //   is that this offsets the perf impact.
        {
            if (_backBuffer.LockedForReading)
            {
                // no swap, the lock is not freed by the reader yet,
                //   a ReadHandle is still in use
                throw new
                    InvalidOperationException("Tried to create a ReadHandle for the Buffer, but another ReadHandle was already given. Maybe the previous ReadHandle was not freed yet?");
            }

            //_backBuffer.LockedForReading = true;

            // clear / reset the 'old' backbuffer,
            //   all data is read, so we should be able to clear this
            BufferSegment.Reset(_backBuffer);

            _backBuffer.LockedForReading = false;

            // set the front buffer to the backbuffer (part 1 of swap)
            var oldFrontBuffer = Interlocked.Exchange(ref _frontBuffer
                                                    , _backBuffer);


            // the old frontBuffer is now the new backbuffer
            _backBuffer                  = oldFrontBuffer;
            _backBuffer.LockedForReading = true;

            return new ReadHandle<T>(_backBuffer);
        }
    }
    
    /// Returns the backing arrays to the array pool if they were allocated
    public void Dispose()
    {
        
        _arrayPool?.Return(_frontBuffer.Buffer, true);
        _arrayPool?.Return(_backBuffer.Buffer);
    }

    // note:
    //   this is a class (and not a struct) because we use CAS swapping
    //     (Interlocked.Exchange) and there is no typed api to
    //     swap refs to structs.
    internal sealed class BufferSegment
    {
        public readonly  T[]  Buffer;
        public           long Index;
        public volatile  bool LockedForReading;
        private readonly int  _bufferLength;

        internal BufferSegment(T[] buffer)
        {
            Buffer        = buffer;
            _bufferLength = buffer.Length;
        }

        /// resets the index and items to default, this is not thread-safe
        /// <remarks>
        /// static method with a `this` reference
        ///    so that il is `call` instead of `callvirt`
        /// </remarks>
        public static void Reset(BufferSegment me)
        {
            if (!me.LockedForReading)
            {
                throw new
                    InvalidOperationException("The BufferSegment is NOT locked for reading, please lock the BufferSegment for reading before resetting");
            }

            // not clearing could be a 'security' issue but is a lot faster
            //me.Buffer.AsSpan().Clear(); 
            me.Index = 0;
        }

        /// adds an item to the Buffer (ordered) this is thread-safe
        /// <remarks>
        /// static method with a `this` reference
        ///    so that il is `call` instead of `callvirt`
        /// </remarks>
        public static bool TryAdd(BufferSegment me, in T value)
        {
            if (me.LockedForReading)
            {
                throw new
                    InvalidOperationException("The BufferSegment is locked for reading, please Dispose the ReadHandle to enable writing to this BufferSegment");
            }

            // fast path
            if (me.Index + 1 >= me._bufferLength)
            {
                //full
                return false;
            }

            var nextIndex = Interlocked.Increment(ref me.Index);
            if (nextIndex >= me._bufferLength)
            {
                // we are full
                return false;
            }

            // yes! we have a spot available
            me.Buffer[nextIndex] = value;
            return true;
        }
    }

    
}

/// A ReadHandle to read the Buffer, this should only be used in a single thread and should be immediately disposed after reading.
public readonly struct ReadHandle<T> : IDisposable
{
    // note the reference is readonly, the _backBuffer itself can
    //   and will be mutated (setting, the LockedForReading state)
    private readonly QueueBuffer<T>.BufferSegment _backBuffer;

    internal ReadHandle(QueueBuffer<T>.BufferSegment backBuffer)
    {
        if (backBuffer.LockedForReading == false)
        {
            throw new
                InvalidOperationException("Given BufferSegment was not locked for reading. Cannot create a valid ReadHandle for this");
        }

        _backBuffer = backBuffer;
    }

    /// Get the contents of the buffer for reading
    public ReadOnlySpan<T> AsSpan()
    {
        if (_backBuffer.LockedForReading == false)
        {
            throw new
                InvalidOperationException("The BufferSegment to read from was not locked for reading. This ReadHandle is invalid. Please dispose all ReadHandles after use, only one ReadHandle can be active at a time.");
        }

        return new ReadOnlySpan<T>(_backBuffer.Buffer
                                 , 0
                                  ,
                                   // due to optimization (and concurrency) the _index can be larger than the
                                   //   length so we need to take that into account when calculating the length
                                   (int)Math.Min(_backBuffer.Index + 1, _backBuffer.Buffer.Length));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _backBuffer.LockedForReading = false;
    }
}