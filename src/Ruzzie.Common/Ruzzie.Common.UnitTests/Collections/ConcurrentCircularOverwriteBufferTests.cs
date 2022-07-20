using System;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;

namespace Ruzzie.Common.UnitTests.Collections;

public class ConcurrentCircularOverwriteBufferTests
{
    [Theory]
    [InlineData(1,             2,             true)]
    [InlineData(0,             long.MaxValue, true)]
    [InlineData(long.MaxValue, long.MinValue, true)] //write header has wrapped around
    [InlineData(long.MinValue, long.MaxValue, false)]
    [InlineData(long.MaxValue, long.MaxValue, false)]
    [InlineData(long.MinValue, long.MinValue, false)]
    [InlineData(0,             0,             false)]
    [InlineData(255,           0,             false)]
    [InlineData(255,           254,           false)]
    [InlineData(255,           -1,            true)]
    public void HasNextTests(long readHeader, long writeHeader, bool expected)
    {
        ConcurrentCircularOverwriteBuffer<object>.HasNext(readHeader, writeHeader).Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorThrowsArgumentExceptionWhenSizeIsLessThanTwo(int size)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Action act = () => new ConcurrentCircularOverwriteBuffer<int>(size);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void CountShouldNotExceedCapacity()
    {
        var size   = 2;
        var buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

        buffer.WriteNext(1);
        buffer.WriteNext(1);
        buffer.WriteNext(1);
        buffer.WriteNext(1);

        buffer.Count.Should().Be(2);
    }

    [Fact]
    public void HasCapacityProperty()
    {
        var size   = 2;
        var buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

        buffer.Capacity.Should().Be(2);
    }

    [Fact]
    public void ReadNextThrowsExceptionWhenEmpty()
    {
        var buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

        Action act = () => buffer.ReadNext();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void SmokeTest()
    {
        var buffer = new ConcurrentCircularOverwriteBuffer<int>();

        buffer.WriteNext(1);
        buffer.WriteNext(2);

        buffer.ReadNext().Should().Be(1);
        buffer.ReadNext().Should().Be(2);
    }

    [Fact]
    public void WriteNextShouldOverwriteValues()
    {
        var buffer = new ConcurrentCircularOverwriteBuffer<int>(2);

        for (var i = 0; i < 10; i++)
        {
            buffer.WriteNext(i);
        }

        buffer.ReadNext().Should().Be(8);
        buffer.ReadNext().Should().Be(9);
    }

    [Fact]
    public void CountShouldReturnAccurateCountWhenReadAndWriteIndexAreMultipleOfCapacityWithRemainder()
    {
        var buffer = new ConcurrentCircularOverwriteBuffer<byte>(3);
        buffer.WriteNext(1);
        buffer.WriteNext(2);

        buffer.ReadNext();

        buffer.Count.Should().Be(1);
    }
}