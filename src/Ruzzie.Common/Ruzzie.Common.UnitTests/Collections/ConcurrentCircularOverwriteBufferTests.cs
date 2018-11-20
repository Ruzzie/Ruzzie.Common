using System;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;

namespace Ruzzie.Common.UnitTests.Collections
{    
    public class ConcurrentCircularOverwriteBufferTests
    {
#if !NET40
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
#endif
        [Fact]
        public void CountShouldNotExceedCapacity()
        {
            var size = 2;
            var buffer = new ConcurrentCircularOverwriteBuffer<int>(size);

            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);
            buffer.WriteNext(1);
            
            buffer.Count.Should().Be(2);
        }
#if !NET40
        [Fact]
        public void ReadNextThrowsExceptionWhenEmpty()
        {
            var buffer = new ConcurrentCircularOverwriteBuffer<int>(2);
            
            Action act = () => buffer.ReadNext();

            act.Should().Throw<Exception>();
        }
#endif      
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
}