namespace Ruzzie.Common.Threading;

/// <summary>
/// A <see cref="IObjectPool{T}"/> of 1.
/// The single object access is wrapped in the <see cref="IObjectPool{T}"/> interface. No locking is done, please make sure that <typeparamref name="T"/>'s methods are all thread safe and it can be reused over multiple threads.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleObjectPool<T> : IObjectPool<T>
{
    private readonly T _singleObject;

    /// <summary>
    /// Ctor.
    /// </summary>
    public SingleObjectPool(T singleObject)
    {
        _singleObject = singleObject;
        PoolSize      = 1;
    }

    /// <summary>
    /// Disposed unmanaged resources. If T is of IDisposable it will dispose all objects in the object pool.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose of resources held by this instance.
            var currentObject = _singleObject as IDisposable;
            currentObject?.Dispose();
        }
    }

    /// <summary>
    /// Disposed unmanaged resources. If T is of IDisposable it will dispose all objects in the object pool
    /// </summary>
    public void Dispose()
    {
        // Dispose of resources held by this instance.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <inheritdoc />
    public TResult ExecuteOnAvailableObject<TResult>(in Func<T, TResult> funcToExecute)
    {
        return funcToExecute(_singleObject);
    }

    /// <inheritdoc />
    public int PoolSize { get; }
}