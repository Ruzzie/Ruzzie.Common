using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Ruzzie.Common.Collections;
using Xunit;

namespace Ruzzie.Common.UnitTests.Collections;

public class FastListTests
{
    [Property]
    public void CtorNoPool(NonNegativeInt minSize)
    {
        //Act
        using var list = new FastList<int>(minSize.Get);

        //Assert
        list.Capacity.Should().BeGreaterOrEqualTo(minSize.Get);
    }

    [Property]
    public void CtorWithPool(NonNegativeInt minSize)
    {
        //Act
        using var list = new FastList<int>(minSize.Get, ArrayPool<int>.Shared);

        //Assert
        list.Capacity.Should().BeGreaterOrEqualTo(minSize.Get);
    }

    [Fact]
    public void AddItemToEmptyList()
    {
        //Arrange
        using var list = new FastList<int>(0);

        //Act
        list.Add(1337);

        //Assert
        list.Count.Should().Be(1);
        list.AsReadOnlySpan()[0].Should().Be(1337);
    }

    [Fact]
    public void AddItemToEmptyWithPoolList()
    {
        //Arrange
        using var list = new FastList<int>(0, ArrayPool<int>.Shared);

        //Act
        list.Add(1337);

        //Assert
        list.Count.Should().Be(1);
        list.AsReadOnlySpan()[0].Should().Be(1337);
    }

    [Fact]
    public void TryCopyToTest()
    {
        //Arrange
        using var list = new FastList<int>(16);
        list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
        var receiver = new int[10];

        //Act
        list.TryCopyTo(receiver).Should().BeTrue();

        //Assert
        receiver.Should().Contain(list.AsSpan().ToArray());
    }

    [Fact]
    public void AddRangeTest()
    {
        //Arrange
        var list = new FastList<Point>(16);

        list.Add(new Point(1, 1));
        list.Add(new Point(2, 2));

        var otherList = new FastList<Point>(2);

        otherList.Add(new Point(3, 3));
        otherList.Add(new Point(4, 4));

        //Act
        list.AddRange(otherList);

        //Assert
        list.Count.Should().Be(4);

        var span = list.AsSpan();
        span[0].Should().Be(new Point(1, 1));
        span[3].Should().Be(new Point(4, 4));

        list.Dispose();
        otherList.Dispose();
    }

    [Fact]
    public void AddTest()
    {
        //Arrange
        var list = new FastList<Point>(16);

        list.Add(new Point(1, 1));
        list.Add(new Point(2, 2));

        //Act & Assert
        list.Count.Should().Be(2);
        var span = list.AsSpan();
        span[0].Should().Be(new Point(1, 1));
        span[1].Should().Be(new Point(2, 2));
    }

    [Fact]
    public void AddWithResizeTest()
    {
        //Arrange
        using var list        = new FastList<int>(2);
        var       oldCapacity = list.Capacity;
        list.Add(1); // make sure count is not zero, to force copy

        //Act
        list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });

        //Assert
        list.Capacity.Should().BeGreaterThan(oldCapacity);
    }

    [Fact]
    public void AddWithResizeWithPoolTest()
    {
        //Arrange
        using var list        = new FastList<int>(16, ArrayPool<int>.Shared);
        var       oldCapacity = list.Capacity;
        list.Add(1); // make sure count is not zero, to force copy

        //Act
        list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });

        //Assert
        list.Capacity.Should().BeGreaterThan(oldCapacity);
    }

    [Property]
    public void AddRangePropertyTest(int[] values)
    {
        //Arrange
        using var list = new FastList<int>(128);
        list.Add(1); // make sure count is not zero, to force copy

        //Act
        list.AddRange(values);

        //Assert
        list.Count.Should().Be(values.Length + 1);
    }

    [Fact]
    public void AddRangeCollectionTest()
    {
        //Arrange
        using var list = new FastList<int>(16);
        list.Add(1);

        var otherCollection = new HashSet<int>();
        otherCollection.Add(2);
        otherCollection.Add(3);

        //Act
        list.AddRange(otherCollection);

        //Assert
        list.Count.Should().Be(3);

        var span = list.AsSpan();
        span[1].Should().Be(2);
        span[2].Should().Be(3);
    }

    [Fact]
    public void AddRangeEnumerableTest()
    {
        //Arrange
        using var list = new FastList<int>(16);
        list.Add(1);

        var enumerable = new Queue<int>();
        enumerable.Enqueue(2);
        enumerable.Enqueue(3);

        //Act
        list.AddRange(enumerable);

        //Assert
        list.Count.Should().Be(3);

        var span = list.AsSpan();
        span[1].Should().Be(2);
        span[2].Should().Be(3);
    }

    [Fact]
    public void AddRangeEnumerableThatIsCollectionTest()
    {
        //Arrange
        using var list = new FastList<int>(16);
        list.Add(1);

        var otherCollection = new HashSet<int>();
        otherCollection.Add(2);
        otherCollection.Add(3);
        IEnumerable<int> enumerable =  otherCollection;

        //Act
        list.AddRange(enumerable);

        //Assert
        list.Count.Should().Be(3);

        var span = list.AsSpan();
        span[1].Should().Be(2);
        span[2].Should().Be(3);
    }

    [Fact]
    public void AddRangeAddSelfShouldDuplicate()
    {
        //Arrange
        using var list = new FastList<int>(16);
        list.Add(1);
        list.Add(2);

        //Act
        list.AddRange(list);

        //Assert
        list.Count.Should().Be(4);

        list.AsSpan().ToArray().Should().BeEquivalentTo(new[] { 1, 2, 1, 2 });
    }

    [Fact]
    public void IndexerGetSuccess()
    {
        //Arrange
        using var list = new FastList<string>(2);
        list.Add("first item");
        list.Add("second item");
        
        //Act
        var first  = list[0];
        var second = list[1];

        //Assert
        first.Should().Be("first item");
        second.Should().Be("second item");
    }

    [Fact]
    public void IndexerGetOutOfRange()
    {
        //Arrange
        using var list = new FastList<int>(2);
        list.Add(20);
        
        var act = () => list[1];
        
        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Property]
    public void IndexerThrowsOutOfRangePropertyTest(int index)
    {
        using var list = new FastList<int>(0);
        
        var act = () => list[index];
        
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}