using SkyNet.DSA.Queues;
using Xunit;

namespace SkyNet.Tests;

public class HeapTests
{
    [Fact]
    public void MaxHeap_ExtractMax_ReturnsHighestPriority()
    {
        var heap = new MaxHeap<string>();
        heap.Insert("Economy Passenger",  1);
        heap.Insert("Gold Passenger",     2);
        heap.Insert("Platinum Passenger", 3);

        Assert.Equal("Platinum Passenger", heap.ExtractMax());
        Assert.Equal("Gold Passenger",     heap.ExtractMax());
        Assert.Equal("Economy Passenger",  heap.ExtractMax());
    }

    [Fact]
    public void MaxHeap_EmptyExtract_ThrowsException()
    {
        var heap = new MaxHeap<string>();
        Assert.Throws<InvalidOperationException>(() => heap.ExtractMax());
    }

    [Fact]
    public void MaxHeap_Count_IsCorrect()
    {
        var heap = new MaxHeap<int>();
        heap.Insert(1, 10);
        heap.Insert(2, 20);
        heap.Insert(3, 30);

        Assert.Equal(3, heap.Count);
        heap.ExtractMax();
        Assert.Equal(2, heap.Count);
    }

    [Fact]
    public void PriorityQueue_SamePriority_FIFOOrder()
    {
        var queue = new PriorityQueue<string>();
        queue.Enqueue("First Economy",  1);
        queue.Enqueue("Second Economy", 1);
        queue.Enqueue("Third Economy",  1);

        // All same priority — should come out in FIFO order
        Assert.Equal("First Economy",  queue.Dequeue());
        Assert.Equal("Second Economy", queue.Dequeue());
        Assert.Equal("Third Economy",  queue.Dequeue());
    }

    [Fact]
    public void PriorityQueue_MixedClasses_PlatinumFirst()
    {
        var queue = new PriorityQueue<string>();
        queue.Enqueue("Economy A",  1);
        queue.Enqueue("Gold A",     2);
        queue.Enqueue("Platinum A", 3);
        queue.Enqueue("Economy B",  1);
        queue.Enqueue("Gold B",     2);

        Assert.Equal("Platinum A", queue.Dequeue());
        Assert.Equal("Gold A",     queue.Dequeue());
        Assert.Equal("Gold B",     queue.Dequeue());
    }

    [Fact]
    public void CircularQueue_EnqueueDequeue_FIFO()
    {
        var queue = new CircularQueue<int>(10);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
    }

    [Fact]
    public void CircularQueue_Full_ThrowsException()
    {
        var queue = new CircularQueue<int>(3);
        queue.Enqueue(1); queue.Enqueue(2); queue.Enqueue(3);
        Assert.Throws<InvalidOperationException>(() => queue.Enqueue(4));
    }

    [Fact]
    public void LinkedStack_PushPop_LIFO()
    {
        var stack = new LinkedStack<string>();
        stack.Push("Bag A");
        stack.Push("Bag B");
        stack.Push("Bag C");

        Assert.Equal("Bag C", stack.Pop());
        Assert.Equal("Bag B", stack.Pop());
        Assert.Equal("Bag A", stack.Pop());
    }

    [Fact]
    public void LinkedStack_Empty_ThrowsException()
    {
        var stack = new LinkedStack<int>();
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }
}
