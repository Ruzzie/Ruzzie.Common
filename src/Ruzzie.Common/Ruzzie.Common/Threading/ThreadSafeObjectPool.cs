using System;
using System.Reflection;
using System.Threading;

namespace Ruzzie.Common.Threading
{
    /// <summary>
    /// Contains a pool of objects that can be used to execute a method thread safe. The pool is created so multiple threads can execute simulanuously, this is done by creating multiple instances of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IDisposable" />
    public class ThreadSafeObjectPool<T> : IDisposable
    {
        private readonly int _poolSize;
        private readonly int _indexMask;
        private readonly T[] _objects;
        private readonly object[] _lockSlots;
        private VolatileLong _index = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeObjectPool{T}"/> class.
        /// </summary>
        /// <param name="createTypeFactory">The factory method to create instances of type T</param>
        /// <param name="poolSize">Size of the pool.</param>
        /// <exception cref="ArgumentNullException">When the createTypeFactory is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">When the poolsize is less than or equal to 0.</exception>
        public ThreadSafeObjectPool(in Func<T> createTypeFactory, int poolSize = 16)
        {
            if (createTypeFactory == null)
            {
                throw new ArgumentNullException(nameof(createTypeFactory));
            }

            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize));
            }

            _poolSize = Numerics.PowerOfTwoHelper.FindNearestPowerOfTwoEqualOrGreaterThan(poolSize);

            _lockSlots = new object[_poolSize];
            _objects = new T[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                _lockSlots[i] = new object();
                _objects[i] = createTypeFactory();
            }
            _indexMask = _poolSize - 1;
        }

        /// <summary>
        /// Executes the method the on available object. This is thread safe.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="funcToExecuteInPool">The function to execute in pool.</param>
        /// <returns>The result from the executed method.</returns>
        public TResult ExecuteOnAvailableObject<TResult>(in Func<T, TResult> funcToExecuteInPool)
        {
            if (funcToExecuteInPool == null)
            {
                throw new ArgumentNullException(nameof(funcToExecuteInPool));
            }

            long currentIndex = _index.CompilerFencedValue;
            do
            {
                currentIndex = currentIndex & _indexMask;

                if (Monitor.TryEnter(_lockSlots[currentIndex]))
                {
                    try
                    {
                        return funcToExecuteInPool.Invoke(_objects[currentIndex]);
                    }
                    finally
                    {
                        Monitor.Exit(_lockSlots[currentIndex]);
                        _index.AtomicIncrement();
                    }
                }
                ++currentIndex;
            } while (true);
        }


        /// <summary>
        /// Disposed unmanaged resources. If T is of IDisposable it will dispose all objects in the object pool.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (typeof(T).IsAssignableFrom(typeof(IDisposable)))
                {
                    // Dispose of resources held by this instance.
                    for (int i = 0; i < _poolSize; i++)
                    {
                        var currentObject = _objects[i] as IDisposable;
                        currentObject?.Dispose();
                    }
                }
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
    }
}
