﻿using System.Diagnostics.CodeAnalysis;

namespace Ruzzie.Common.Threading;

/// <summary>
/// Contains methods for performing volatile memory operations.
/// For portability reasons this wraps either System.Threading.Volatile or implements Thread.MemoryBarrier() on framework versions that do not support that class.
/// </summary>
public static class Volatile
{
    /// <summary>
    /// Writes the specified object reference to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.
    /// </summary>
    /// <param name="location">The field where the object reference is written.</param>
    /// <param name="value">The object reference to write. The reference is written immediately so that it is visible to all processors in the computer.</param>
    /// <typeparam name="T">The type of field to write. This must be a reference type, not a value type.</typeparam>
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public static void Write<T>(ref T location, in T value) where T : class
    {
#if NET40 || PORTABLE
            System.Threading.Thread.MemoryBarrier();
            location = value;
#else
        System.Threading.Volatile.Write(ref location, value);
#endif
    }

    /// <summary>
    /// Writes the specified object reference to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.
    /// </summary>
    /// <param name="location">The field where the object reference is written.</param>
    /// <param name="value">The object reference to write. The reference is written immediately so that it is visible to all processors in the computer.</param>
    /// <typeparam name="T">The type of field to write.</typeparam>
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public static void WriteValueType<T>(ref T location, T value) where T : struct
    {
#if NET40 || PORTABLE
            System.Threading.Thread.MemoryBarrier();
#else
        Interlocked.MemoryBarrier();
#endif
        location = value;
    }

    /// <summary>
    /// Reads the value reference from the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.
    /// </summary>
    /// <param name="location">The field to read.</param>
    /// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
    /// <returns>The reference to <typeparamref name="T"/> that was read. This reference is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache</returns>
    public static T Read<T>(ref T location) where T : class
    {
#if NET40 || PORTABLE
            T value = location;
            System.Threading.Thread.MemoryBarrier();
            return value;
#else
        return System.Threading.Volatile.Read(ref location);
#endif
    }

    /// <summary>
    /// Reads the value reference from the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.
    /// </summary>
    /// <param name="location">The field to read.</param>
    /// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
    /// <returns>The reference to <typeparamref name="T"/> that was read. This reference is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache</returns>
    public static T ReadValueType<T>(ref T location) where T : struct
    {
        T value = location;
#if NET40 || PORTABLE
            System.Threading.Thread.MemoryBarrier();
#else
        Interlocked.MemoryBarrier();
#endif
        return value;
    }
}