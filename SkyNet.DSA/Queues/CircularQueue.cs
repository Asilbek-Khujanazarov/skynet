namespace SkyNet.DSA.Queues;

/// <summary>
/// Circular Queue (Ring Buffer) — FIFO for boarding gate management.
/// Reference: Sedgewick, R. (1983) Algorithms. Addison-Wesley, pp. 45–48.
/// Enqueue: O(1) | Dequeue: O(1) | Space: O(n)
/// </summary>
public class CircularQueue<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _tail;
    private int _count;
    private readonly int _capacity;

    public int Count => _count;
    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _capacity;

    public CircularQueue(int capacity = 256)
    {
        _capacity = capacity;
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public void Enqueue(T item)
    {
        if (IsFull) throw new InvalidOperationException("Queue is full (boarding gate capacity reached).");
        _buffer[_tail] = item;
        _tail = (_tail + 1) % _capacity;
        _count++;
    }

    public T Dequeue()
    {
        if (IsEmpty) throw new InvalidOperationException("Queue is empty.");
        var item = _buffer[_head];
        _buffer[_head] = default!;
        _head = (_head + 1) % _capacity;
        _count--;
        return item;
    }

    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("Queue is empty.");
        return _buffer[_head];
    }

    public T[] ToArray()
    {
        var result = new T[_count];
        for (int i = 0; i < _count; i++)
            result[i] = _buffer[(_head + i) % _capacity];
        return result;
    }

    public void Clear()
    {
        _head = 0; _tail = 0; _count = 0;
    }
}
