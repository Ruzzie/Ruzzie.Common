using System;
using System.Collections.Generic;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;

namespace Ruzzie.Common.UnitTests.Collections;

//We only want to test if the readonlyset wraps the underlying set, not testing the actual set functionality
// so these are 'shallow' call tests only
public class ReadOnlySetTests
{
    public class ConstructorTests
    {
        [Fact]
        // ReSharper disable once InconsistentNaming
        public void ISetCtorWrapsSet()
        {
            //Arrange
            ISet<string> mySet = new SortedSet<string>(new[] {"A", "B", "C"});

            //Act
            var readOnlySet = new ReadOnlySet<string>(mySet);

            //Assert
            readOnlySet.Count.Should().Be(mySet.Count);
        }

        [Fact]
        public void HashSetCtorWrapsAndCopiesSet()
        {
            //Arrange
            var mySet = new HashSet<string>(new[] {"A", "B", "C"});

            //Act
            var readOnlySet = new ReadOnlySet<string>(mySet);
            mySet.Add("D");

            //Assert
            readOnlySet.Count.Should().Be(mySet.Count - 1);
        }

        [Fact]
        public void HashSetCtorThrowsExceptionWhenInputIsNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new ReadOnlySet<int>(null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void ISetCtorThrowsExceptionWhenInputIsNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new ReadOnlySet<int>(((ISet<int>) null));

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void EnumerableCtorThrowsExceptionWhenInputIsNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new ReadOnlySet<int>(((IEnumerable<int>) null));
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void EnumerableCtorCreateHashSet()
        {
            //Arrange
            var                 hashSet = new HashSet<string>(new[] {"A", "B", "C"});
            IEnumerable<string> mySet   = hashSet;

            //Act
            var readOnlySet = new ReadOnlySet<string>(mySet);
            hashSet.Add("D");

            //Assert
            readOnlySet.Count.Should().Be(hashSet.Count - 1);
        }

        [Fact]
        public void EnumerableAndComparerCtorDoesNotThrowsExceptionWhenComparer()
        {
            var readOnlySet = new ReadOnlySet<string>(new[] {"A"}, null);

            readOnlySet.Should().NotBeNull();
        }

        [Fact]
        public void EnumerableAndComparerCtorCreateHashSet()
        {
            //Arrange
            var                 hashSet = new HashSet<string>(new[] {"A", "B", "C"});
            IEnumerable<string> mySet   = hashSet;

            //Act
            var readOnlySet = new ReadOnlySet<string>(mySet, StringComparer.OrdinalIgnoreCase);
            hashSet.Add("D");

            //Assert
            readOnlySet.Count.Should().Be(hashSet.Count - 1);
        }
    }

    public class ModificationTests
    {
        [Fact]
        public void AddThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).Add("B");
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void RemoveThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).Remove("B");
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void ClearThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).Clear();
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void UnionWithThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).UnionWith(new[] {"B"});
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void InterSectWithThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).IntersectWith(new[] {"B"});
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void SymmetricExceptWithThrowsException()
        {
            Action act = () => new ReadOnlySet<string>(new[] {"A"}).SymmetricExceptWith(new[] {"B"});
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void PropertyIsReadOnlyIsTrue()
        {
            new ReadOnlySet<string>(new[] {"A"}).IsReadOnly.Should().BeTrue();
        }
    }

    public class ReadTests
    {
        private readonly ReadOnlySet<string> _readOnlySet =
            new ReadOnlySet<string>(new HashSet<string>(new[] {"A", "B"}));

        [Fact]
        public void GetEnumeratorTest()
        {
            _readOnlySet.GetEnumerator().Should().NotBeNull();
        }

        [Fact]
        public void ContainsTest()
        {
            _readOnlySet.Should().Contain("A");
        }

        [Fact]
        public void CopyToTest()
        {
            //Act
            string[] arr = new string[2];
            _readOnlySet.CopyTo(arr, 0);

            //Assert
            arr[0].Should().NotBeNull();
            arr[1].Should().NotBeNull();
        }

        [Fact]
        public void IsSubsetOfTest()
        {
            _readOnlySet.IsSubsetOf(new[] {"A", "B", "C"}).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOfTest()
        {
            _readOnlySet.IsProperSupersetOf(new[] {"A", "B", "C"}).Should().BeFalse();
        }

        [Fact]
        public void IsSupersetOfTest()
        {
            _readOnlySet.IsSupersetOf(new[] {"A", "B"}).Should().BeTrue();
        }

        [Fact]
        public void IsProperSupersetOfTest()
        {
            _readOnlySet.IsProperSupersetOf(new[] {"A", "B"}).Should().BeFalse();
        }

        [Fact]
        public void OverlapsTest()
        {
            _readOnlySet.Overlaps(new[] {"A", "B", "Q"}).Should().BeTrue();
        }

        [Fact]
        public void SetEqualsTest()
        {
            _readOnlySet.Overlaps(new[] {"A", "B"}).Should().BeTrue();
        }

        [Fact]
        public void CountTest()
        {
            _readOnlySet.Count.Should().Be(2);
        }
    }
}