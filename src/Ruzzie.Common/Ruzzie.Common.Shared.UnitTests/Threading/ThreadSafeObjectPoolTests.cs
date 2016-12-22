using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if ! PORTABLE
using System;
using Moq;
#endif
using NUnit.Framework;
using Ruzzie.Common.Threading;

namespace Ruzzie.Common.Shared.UnitTests.Threading
{
    [TestFixture]
    public class ThreadSafeObjectPoolTests
    {
        [Test]
        public void SmokeTest()
        {
            ThreadSafeObjectPool<SHA1> sha1ObjectPool = new ThreadSafeObjectPool<SHA1>(SHA1.Create, 8);

            Parallel.For(0, 100000, i => { sha1ObjectPool.ExecuteOnAvailableObject(sha1 => sha1.ComputeHash(Encoding.Unicode.GetBytes("ThreadSafeObjectPoolTests SmokeTest" + i))); });
        }

#if ! PORTABLE
        [Test]
        public void DisposeTest()
        {
            Mock<IDisposable> disposableMock = new Mock<IDisposable>();
            var sha1ObjectPool = new ThreadSafeObjectPool<IDisposable>(() => disposableMock.Object, 1);

            //Act
            sha1ObjectPool.Dispose();

            //Assert
            disposableMock.Verify(disposable => disposable.Dispose(), Times.Once);
        }
#endif
        [Test]
        public void ContentionTest()
        {
            int objectCount = 1;
            ThreadSafeObjectPool<ContentionObject> pool = new ThreadSafeObjectPool<ContentionObject>(() => new ContentionObject((objectCount++).ToString()), 2);

            //Each execution should be handled by a different object
            var resultOne = Task.Run(() => pool.ExecuteOnAvailableObject(o => o.ExecuteAndHoldLock(100)));
            var resultTwo = Task.Run(() => pool.ExecuteOnAvailableObject(o => o.ExecuteAndHoldLock(100)));

            Assert.That(resultOne.Result, Is.Not.EqualTo(resultTwo.Result));
        }

        class ContentionObject
        {
            private string Name { get; }

            public ContentionObject(string name)
            {
                Name = name;
            }

            public string ExecuteAndHoldLock(int sleepTimeInMillis)
            {
                Thread.Sleep(sleepTimeInMillis);
                return Name;
            }
        }
    }
}
