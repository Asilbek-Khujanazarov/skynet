namespace SkyNet.DSA.Queues;

/// <summary>
/// Priority Queue built on top of MaxHeap — for passenger check-in ordering.
/// Platinum (3) > Gold (2) > Economy (1). Ties broken by arrival order (timestamp).
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press.
/// Enqueue: O(log n) | Dequeue: O(log n)
/// </summary>
public class PriorityQueue<T>
{
    private readonly MaxHeap<PriorityItem<T>> _heap;
    private int _insertOrder;

    public int Count => _heap.Count;
    public bool IsEmpty => _heap.IsEmpty;

    public PriorityQueue(int capacity = 256)
    {
        _heap = new MaxHeap<PriorityItem<T>>(capacity);
        _insertOrder = 0;
    }

    public void Enqueue(T item, int priority)
    {
        // Composite key: priority * 1_000_000 - insertOrder (FIFO within same priority)
        int compositeKey = priority * 1_000_000 - _insertOrder++;
        _heap.Insert(new PriorityItem<T>(item, priority, _insertOrder), compositeKey);
    }

    public T Dequeue()
    {
        return _heap.ExtractMax().Item;
    }

    public T Peek() => _heap.Peek().Item;

    public int PeekPriority() => _heap.Peek().Priority;

    public T[] ToArray()
    {
        var raw = _heap.ToArray();
        var result = new T[raw.Length];
        for (int i = 0; i < raw.Length; i++) result[i] = raw[i].Item;
        return result;
    }
}

public class PriorityItem<T>
{
    public T Item { get; }
    public int Priority { get; }
    public int InsertOrder { get; }

    public PriorityItem(T item, int priority, int insertOrder)
    {
        Item = item;
        Priority = priority;
        InsertOrder = insertOrder;
    }
}
