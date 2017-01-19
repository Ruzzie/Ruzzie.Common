using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruzzie.Common.Collections;

namespace Ruzzie.Common.UnitTests.Collections
{
    //We only want to test if the readonlyset wraps the underlying set, not testing the actual set functionality
    // so these are 'shallow' call tests only
    public class ReadOnlySetTests
    {
        [TestFixture]
        public class ConstructorTests
        {
            [Test]
            // ReSharper disable once InconsistentNaming
            public void ISetCtorThrowsExceptionWhenInputIsNull()
            {
                Assert.That(() => new ReadOnlySet<int>( ((ISet<int>) null)), Throws.Exception);
            }

            [Test]
            // ReSharper disable once InconsistentNaming
            public void ISetCtorWrapsSet()
            {
                //Arrange
                ISet<string> mySet = new SortedSet<string>(new [] {"A","B","C"});

                //Act
                var readOnlySet = new ReadOnlySet<string>(mySet);

                //Assert
                Assert.That(readOnlySet.Count, Is.EqualTo(mySet.Count));
            }

            [Test]
            public void HashSetCtorThrowsExceptionWhenInputIsNull()
            {
                Assert.That(() => new ReadOnlySet<int>(null), Throws.Exception);
            }

            [Test]
            public void HashSetCtorWrapsAndCopiesSet()
            {
                //Arrange
                HashSet<string> mySet = new HashSet<string>(new[] { "A", "B", "C" });
                
                //Act
                var readOnlySet = new ReadOnlySet<string>(mySet);
                mySet.Add("D");

                //Assert
                Assert.That(readOnlySet.Count, Is.EqualTo(mySet.Count -1));
            }

            [Test]
            public void EnumerableCtorThrowsExceptionWhenInputIsNull()
            {
                Assert.That(() => new ReadOnlySet<int>(((IEnumerable<int>)null)), Throws.Exception);
            }

            [Test]
            public void EnumerableCtorCreateHashSet()
            {
                //Arrange
                HashSet<string> hashSet = new HashSet<string>(new[] { "A", "B", "C" });
                IEnumerable<string> mySet = hashSet;

                //Act
                var readOnlySet = new ReadOnlySet<string>(mySet);
                hashSet.Add("D");

                //Assert
                Assert.That(readOnlySet.Count, Is.EqualTo(hashSet.Count - 1));
            }

            [Test]
            public void EnumerableAndComparerCtorDoesNotThrowsExceptionWhenComparer()
            {
                Assert.That(() => new ReadOnlySet<string>(new [] {"A"},null), Throws.Nothing);
            }

            [Test]
            public void EnumerableAndComparerCtorCreateHashSet()
            {
                //Arrange
                HashSet<string> hashSet = new HashSet<string>(new[] { "A", "B", "C" });
                IEnumerable<string> mySet = hashSet;

                //Act
                var readOnlySet = new ReadOnlySet<string>(mySet, StringComparer.OrdinalIgnoreCase);
                hashSet.Add("D");

                //Assert
                Assert.That(readOnlySet.Count, Is.EqualTo(hashSet.Count - 1));
            }
        }

        [TestFixture]
        public class ModificationTests
        {
            [Test]
            public void AddThrowsException()
            {
                Assert.That(()=> new ReadOnlySet<string>(new [] {"A"}).Add("B"), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void RemoveThrowsException()
            {
                Assert.That(() => new ReadOnlySet<string>(new[] { "A" }).Remove("B"), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void ClearThrowsException()
            {
                Assert.That(() => new ReadOnlySet<string>(new[] { "A" }).Clear(), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void UnionWithThrowsException()
            {
                Assert.That(() => new ReadOnlySet<string>(new[] { "A" }).UnionWith(new [] {"B"}), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void InterSectWithThrowsException()
            {
                Assert.That(() => new ReadOnlySet<string>(new[] { "A" }).IntersectWith(new[] { "B" }), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void SymmetricExceptWithThrowsException()
            {
                Assert.That(() => new ReadOnlySet<string>(new[] { "A" }).SymmetricExceptWith(new[] { "B" }), Throws.TypeOf<NotSupportedException>());
            }

            [Test]
            public void PropertyIsReadOnlyIsTrue()
            {
                Assert.That(new ReadOnlySet<string>(new[] { "A" }).IsReadOnly, Is.True);
            }
        }

        [TestFixture]
        public class ReadTests
        {
            private readonly ReadOnlySet<string> _readOnlySet = new ReadOnlySet<string>(new HashSet<string>(new[] { "A", "B" }));

            [Test]
            public void GetEnumeratorTest()
            {
              Assert.That(_readOnlySet.GetEnumerator(), Is.Not.Null);
            }

            [Test]
            public void ContainsTest()
            {
                Assert.That(_readOnlySet.Contains("A"),Is.True);
            }

            [Test]
            public void CopyToTest()
            {
                //Act
                string[] arr = new string[2];
                _readOnlySet.CopyTo(arr,0);

                //Assert
                Assert.That(arr[0], Is.Not.Null);
                Assert.That(arr[1], Is.Not.Null);
            }

            [Test]
            public void IsSubsetOfTest()
            {
                Assert.That(_readOnlySet.IsSubsetOf(new [] {"A","B","C"}), Is.True);
            }

            [Test]
            public void IsProperSubsetOfTest()
            {
                Assert.That(_readOnlySet.IsProperSupersetOf(new[] { "A", "B", "C" }), Is.False);
            }

            [Test]
            public void IsSupersetOfTest()
            {
                Assert.That(_readOnlySet.IsSupersetOf(new[] { "A","B" }), Is.True);
            }

            [Test]
            public void IsProperSupersetOfTest()
            {
                Assert.That(_readOnlySet.IsProperSupersetOf(new[] { "A","B" }), Is.False);
            }

            [Test]
            public void OverlapsTest()
            {
                Assert.That(_readOnlySet.Overlaps(new[] { "A", "B","Q" }), Is.True);
            }

            [Test]
            public void SetEqualsTest()
            {
                Assert.That(_readOnlySet.Overlaps(new[] { "A", "B" }), Is.True);
            }

            [Test]
            public void CountTest()
            {
                Assert.That(_readOnlySet.Count, Is.EqualTo(2));
            }
        }
    }
}
