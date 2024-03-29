﻿using Volatile = System.Threading.Volatile;

namespace Ruzzie.Common.Collections;

/// A thread-safe Buffer as a Queue for multiple producers single consumer (that batch reads) 
public interface IQueueBuffer<T> : IDisposable
{
    /// Add an item to the queue, this is thread-safe
    bool TryAdd(in T value);

    /// Snapshots the writes up until now and returns a ReadHandle from which
    ///   you can read all the items.
    /// The ReadHandle should be disposed after use to free the ReadHandle.
    /// Supports only one ReadHandle at a time.
    /// <remarks>
    /// Effectively this methods swaps the front and back buffer to snapshot
    ///   (and let writers continue writing).
    /// </remarks>
    ReadHandle ReadBuffer();


    /// A ReadHandle to read the Buffer, this should only be used in a single thread and should be immediately disposed after reading.
    public readonly ref struct ReadHandle // : IDisposable
    {
        /// The buffer
        public readonly ReadOnlySpan<T> Data;

        private readonly Ref<bool> _lockedForReadingRef;

        /// Creates a new <see cref="ReadHandle"/> wrapping the given <see paramref="backBuffer"/>.
        /// The <see paramref="lockedForReadingRef"/> is expected to be initialized to <value>true</value> else it will throw a <see cref="InvalidOperationException"/>.
        /// <remarks> The <see paramref="lockedForReadingRef"/> will be set to false when this handle is disposed.</remarks>
        public ReadHandle(ReadOnlySpan<T> backBuffer, Ref<bool> lockedForReadingRef)
        {
            if (lockedForReadingRef.Value == false)
            {
                throw new
                    InvalidOperationException("Given Buffer was not locked for reading. Cannot create a valid ReadHandle for this");
            }

            Data                 = backBuffer;
            _lockedForReadingRef = lockedForReadingRef;
        }

        /// <summary>
        /// Disposes the handle
        /// </summary>
        public void Dispose()
        {
            Volatile.Write(ref _lockedForReadingRef.Value, false);
            // should we clear the array contents (for security and easier debugging purposes) ...?
        }
    }
}