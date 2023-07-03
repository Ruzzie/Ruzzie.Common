using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Collections;

public class QueueBufferTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public QueueBufferTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void MaskWriteHeaderTests()
    {
        int writeHeader = 0; // buffer select 0, nat. index 0
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(0);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(0);

        //swap
        writeHeader = QueueBuffer.SwapAndResetWriteHeader(writeHeader);
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(1);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(0);

        //swap
        writeHeader = QueueBuffer.SwapAndResetWriteHeader(writeHeader);
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(0);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(0);

        //increment and swap
        writeHeader++;
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(0);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(1);

        writeHeader++;
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(0);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(2);

        //swap
        writeHeader = QueueBuffer.SwapAndResetWriteHeader(writeHeader);
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(1);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(0);

        //increment and swap
        writeHeader++;
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(1);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(1);

        writeHeader++;
        QueueBuffer.SelectBuffer(writeHeader).Should().Be(1);
        QueueBuffer.SelectIndex(writeHeader).Should().Be(2);
    }


    [Fact]
    public void ReadBufferSpanIsSizeOfItemCount()
    {
        //Arrange
        var queue = new QueueBuffer<string>();
        //Act
        queue.TryAdd("first").Should().BeTrue();
        queue.TryAdd("second").Should().BeTrue();

        //Assert
        using var readHandle = queue.ReadBuffer();
        var       items      = readHandle.AsSpan();
        items.Length.Should().Be(2);
    }

    [Fact]
    public void ReadsAddedItem()
    {
        //Arrange
        var queue = new QueueBuffer<string>();
        //Act
        queue.TryAdd("first").Should().BeTrue();

        //Assert
        using var readHandle = queue.ReadBuffer();
        var       items      = readHandle.AsSpan();
        items.Length.Should().Be(1);
        items[0].Should().Be("first");
    }

    [Fact]
    public void AddsAndReadsInOrder()
    {
        //Arrange
        var queue = new QueueBuffer<string>();
        //Act
        queue.TryAdd("first").Should().BeTrue();
        queue.TryAdd("second").Should().BeTrue();

        //Assert
        using var readHandle = queue.ReadBuffer();
        var       items      = readHandle.AsSpan();

        items[0].Should().Be("first");
        items[1].Should().Be("second");
    }

    [Fact]
    public void TryAddReturnsFalseWhenFull()
    {
        //Arrange
        var queue = new QueueBuffer<string>(1);
        queue.TryAdd("first").Should().BeTrue();

        //Act & Assert
        queue.TryAdd("second").Should().BeFalse();
    }

    [Fact]
    public void CanAddToWhenReading()
    {
        //Arrange
        var queue = new QueueBuffer<string>(2);
        queue.TryAdd("first").Should().BeTrue();

        using var readHandle = queue.ReadBuffer();

        //Act & Assert
        queue.TryAdd("second").Should().BeTrue();
    }

    [Fact]
    public void OnlyOneConsumer()
    {
        //Arrange
        using var queue = new QueueBuffer<string>(1);
        queue.TryAdd("first").Should().BeTrue();
        using var readHandleOne = queue.ReadBuffer();

        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.ReadBuffer());
    }


    [Fact]
    public void CanDisposeWhenNotReading()
    {
        //Arrange
        var queue = new QueueBuffer<string>(2);
        queue.TryAdd("first").Should().BeTrue();

        //Act & Assert
        //  no exceptions
        queue.Dispose();
    }

    [Fact]
    public void MultipleProducersSingleConsumerCheckAllData()
    {
        //Arrange
        using var       queue                 = new QueueBuffer<string>();
        HashSet<string> uniqueItemsToValidate = new HashSet<string>();

        int itemsPerProducer = 512;

        var writeTaskOne = new Task(() =>
                                    {
                                        for (int i = 0; i < itemsPerProducer; i++)
                                        {
                                            queue.TryAdd("one-" + i).Should().BeTrue();
                                            //Thread.Sleep(1);
                                        }
                                    }
                                   );

        var writeTaskTwo = new Task(() =>
                                    {
                                        for (int i = 0; i < itemsPerProducer; i++)
                                        {
                                            queue.TryAdd("two-" + i).Should().BeTrue();
                                            //Thread.Sleep(1);
                                        }
                                    }
                                   );

        //Act
        writeTaskOne.Start();
        writeTaskTwo.Start();

        //  read while the writers are writing
        while (!writeTaskOne.IsCompleted || !writeTaskTwo.IsCompleted)
        {
            ReadAvailableItems();
            //Thread.Sleep(1);
        }

        writeTaskOne.Wait();
        writeTaskTwo.Wait();

        // they are done
        //  final read
        ReadAvailableItems();

        //Assert
        uniqueItemsToValidate.Count.Should().Be(1024);

        void ReadAvailableItems()
        {
            using var readHandle = queue.ReadBuffer();
            var       items      = readHandle.AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                uniqueItemsToValidate.Add(item);
            }
        }
    }
}