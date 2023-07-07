using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Ruzzie.Common.Collections;
using Xunit;
using Xunit.Abstractions;

namespace Ruzzie.Common.UnitTests.Collections;

public class QueueBufferSWTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public QueueBufferSWTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SwapAndIncrementProducerTests()
    {
        var producer = 2ul; // 2 elements buffer select buffer 0

        producer = QueueBufferSW.IncrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(1);

        producer = QueueBufferSW.DecrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(0);

        //SWAP
        producer = QueueBufferSW.SwapAndResetWriteIndex(producer);

        QueueBufferSW.SelectIndex(producer).Should().Be(0);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(0);
        QueueBufferSW.SelectCurrentBufferIdx(producer).Should().Be(1);


        producer = QueueBufferSW.IncrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(1);


        //SWAP
        producer = QueueBufferSW.SwapAndResetWriteIndex(producer);

        QueueBufferSW.SelectCurrentBufferIdx(producer).Should().Be(0);
        QueueBufferSW.SelectIndex(producer).Should().Be(0);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(1); // producers count is not blotted out
    }

    [Fact]
    public void ClearProducersMask()
    {
        var producer = 2ul; // 2 elements buffer select buffer 0

        producer = QueueBufferSW.SwapAndResetWriteIndex(producer);

        producer = QueueBufferSW.IncrementProducer(producer);
        producer = QueueBufferSW.IncrementProducer(producer);
        producer++; // nextIndex
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(2);

        producer = producer & QueueBufferSW.CLEAR_PRODUCERS_MASK;

        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(0);
        QueueBufferSW.SelectIndex(producer).Should().Be(1);
        QueueBufferSW.SelectCurrentBufferIdx(producer).Should().Be(1);
    }


    [Fact]
    public void ProducerSwapTests()
    {
        var producer = 0ul;

        QueueBufferSW.SelectCurrentBufferIdx(producer).Should().Be(0);

        producer = QueueBufferSW.SwapAndResetWriteIndex(producer);
        QueueBufferSW.SelectCurrentBufferIdx(producer).Should().Be(1);

        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(0);

        producer = QueueBufferSW.IncrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(1);

        producer = QueueBufferSW.IncrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(2);

        producer = QueueBufferSW.DecrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(1);
        producer = QueueBufferSW.DecrementProducer(producer);
        QueueBufferSW.SelectCurrentProducerCount(producer).Should().Be(0);
    }

    [Fact]
    public void SingleThreadedProduceConsumeIsCorrect()
    {
        //Arrange
        var capacity = 1024;
        var buffer   = new QueueBufferSW<string>(capacity);

        //ACT
        //  Fill the buffer
        for (int i = 0; i < capacity; i++)
        {
            buffer.TryAdd(i.ToString()).Should().BeTrue($"failed while adding {i}");
        }

        //  Drain the buffer
        using var readHandle = buffer.ReadBuffer();

        //Assert: We should have exactly x number of unique items
        readHandle.Data.ToArray().ToHashSet().Count.Should().Be(1024);
    }

    [Fact]
    public void SingleThreadedFlipFlopConsumeIsCorrect()
    {
        //Arrange
        var capacity       = 1024;
        var buffer         = new QueueBufferSW<string>(capacity);
        var allUniqueItems = new HashSet<string>();

        //ACT
        //  Fill the buffer
        for (int i = 0; i < capacity; i++)
        {
            buffer.TryAdd(i.ToString()).Should().BeTrue($"failed while adding {i}");
            buffer.TryAdd(i + capacity.ToString()).Should().BeTrue($"failed while adding {i + capacity}");

            {
                //  Drain the buffer
                using var readHandle = buffer.ReadBuffer();
                var       data       = readHandle.Data;

                for (var readIdx = 0; readIdx < data.Length; readIdx++)
                {
                    allUniqueItems.Add(data[readIdx])
                                  .Should()
                                  .BeTrue("failed adding read value, duplicate detected");
                }
            }
        }

        allUniqueItems.Count.Should().Be(capacity * 2);
    }


    [Fact]
    public void AddOneItem()
    {
        new QueueBufferSW<int>(2).TryAdd(1).Should().BeTrue();
    }

    [Fact]
    public void ReadBufferSpanIsSizeOfItemCount()
    {
        //Arrange
        var queue = new QueueBufferSW<string>();
        //Act
        queue.TryAdd("first").Should().BeTrue();
        queue.TryAdd("second").Should().BeTrue();

        //Assert
        using var readHandle = queue.ReadBuffer();
        var       items      = readHandle.Data;
        items.Length.Should().Be(2);
    }

    [Fact]
    public void ReadsAddedItem()
    {
        //Arrange
        var queue = new QueueBufferSW<string>();
        //Act
        queue.TryAdd("first").Should().BeTrue();

        //Assert
        using var readHandle = queue.ReadBuffer();
        var       items      = readHandle.Data;
        items.Length.Should().Be(1);
        items[0].Should().Be("first");
    }

    [Fact]
    public void AddsAndReadsInOrder()
    {
        //Arrange
        var queue = new QueueBufferSW<string>();

        //Act
        queue.TryAdd("first").Should().BeTrue();
        queue.TryAdd("second").Should().BeTrue();

        //Assert
        {
            using var readHandle = queue.ReadBuffer(); // swap
            var       items      = readHandle.Data;

            items[0].Should().Be("first");
            items[1].Should().Be("second");

            readHandle.Dispose();
        }

        //Act
        queue.TryAdd("2_first").Should().BeTrue();
        queue.TryAdd("2_second").Should().BeTrue();

        //Assert
        {
            using var readHandle2 = queue.ReadBuffer(); // swap
            var       items       = readHandle2.Data;

            items[0].Should().Be("2_first");
            items[1].Should().Be("2_second");
        }
    }

    [Fact]
    public void TryAddReturnsFalseWhenFull()
    {
        //Arrange
        var queue = new QueueBufferSW<string>(1);
        queue.TryAdd("first").Should().BeTrue();

        //Act & Assert
        queue.TryAdd("second").Should().BeFalse();
    }

    [Fact]
    public void CanAddToWhenReading()
    {
        //Arrange
        var queue = new QueueBufferSW<string>(2);
        queue.TryAdd("first").Should().BeTrue();

        using var readHandle = queue.ReadBuffer();

        //Act & Assert
        queue.TryAdd("second").Should().BeTrue();
    }

    [Fact]
    public void OnlyOneConsumer()
    {
        //Arrange
        using var queue = new QueueBufferSW<string>(1);
        queue.TryAdd("first").Should().BeTrue();
        using var readHandleOne = queue.ReadBuffer();

        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.ReadBuffer());
    }


    [Fact]
    public void CanDisposeWhenNotReading()
    {
        //Arrange
        var queue = new QueueBufferSW<string>(2);
        queue.TryAdd("first").Should().BeTrue();

        //Act & Assert
        //  no exceptions
        queue.Dispose();
    }

    [Fact]
    public void MultipleProducersSingleConsumerCheckAllData()
    {
        //Arrange
        using var       queue                 = new QueueBufferSW<string>();
        HashSet<string> uniqueItemsToValidate = new HashSet<string>();

        int itemsPerProducer = 512;

        var writeTaskOne = new Task(() =>
                                    {
                                        for (int i = 0; i < itemsPerProducer; i++)
                                        {
                                            queue.TryAdd("one-" + (i + 1).ToString().PadLeft(4, '0')).Should().BeTrue();
                                            // Thread.Sleep(1);
                                        }
                                    }
                                   );

        var writeTaskTwo = new Task(() =>
                                    {
                                        for (int i = 0; i < itemsPerProducer; i++)
                                        {
                                            queue.TryAdd("two-" + (i + 1).ToString().PadLeft(4, '0')).Should().BeTrue();
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
        /*foreach (var s in uniqueItemsToValidate.OrderBy(x => x))
        {
            _testOutputHelper.WriteLine(s);
        }*/

        uniqueItemsToValidate.Count.Should().Be(1024);


        void ReadAvailableItems()
        {
            using var readHandle = queue.ReadBuffer();
            var       items      = readHandle.Data;
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                uniqueItemsToValidate.Add(item);
            }
        }
    }
}