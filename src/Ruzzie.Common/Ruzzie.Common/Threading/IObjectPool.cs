using System;

namespace Ruzzie.Common.Threading;

/// <summary>
/// Interface to execute actions in an ObjectPool
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObjectPool<T> : IDisposable
{
    /// <summary>
    /// Executes the method the on an available object. The implementation is expected to be thread safe.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="funcToExecuteInPool">The function to execute in pool.</param>
    /// <returns>The result from the executed method.</returns>
    TResult ExecuteOnAvailableObject<TResult>(in Func<T, TResult> funcToExecuteInPool);

    /// <summary>
    /// The PoolSize
    /// </summary>
    int PoolSize { get; }
}