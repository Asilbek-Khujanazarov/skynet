namespace SkyNet.DSA.Queues;

/// <summary>
/// Generic Max-Heap data structure.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 161–168.
/// Insert: O(log n) | ExtractMax: O(log n) | Peek: O(1)
/// </summary>
public class MaxHeap<T>
{
    private (T item, int priority)[] _heap;
    private int _size;
    private int _capacity;

    public int Count => _size;
    public bool IsEmpty => _size == 0;

    public MaxHeap(int capacity = 256)
    {
        _capacity = capacity;
        _heap = new (T, int)[capacity];
        _size = 0;
    }

    public void Insert(T item, int priority)
    {
        if (_size >= _capacity) Resize();
        _heap[_size] = (item, priority);
        HeapifyUp(_size++);
    }

    public T ExtractMax()
    {
        if (IsEmpty) throw new InvalidOperationException("Heap is empty.");
        var max = _heap[0].item;
        _heap[0] = _heap[--_size];
        HeapifyDown(0);
        return max;
    }

    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("Heap is empty.");
        return _heap[0].item;
    }

    public int PeekPriority()
    {
        if (IsEmpty) throw new InvalidOperationException("Heap is empty.");
        return _heap[0].priority;
    }

    public T[] ToArray()
    {
        var result = new T[_size];
        for (int i = 0; i < _size; i++) result[i] = _heap[i].item;
        return result;
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_heap[parent].priority >= _heap[i].priority) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1, right = 2 * i + 2, largest = i;
            if (left < _size && _heap[left].priority > _heap[largest].priority) largest = left;
            if (right < _size && _heap[right].priority > _heap[largest].priority) largest = right;
            if (largest == i) break;
            Swap(i, largest);
            i = largest;
        }
    }

    private void Swap(int a, int b) => (_heap[a], _heap[b]) = (_heap[b], _heap[a]);

    private void Resize()
    {
        _capacity *= 2;
        var newHeap = new (T, int)[_capacity];
        for (int i = 0; i < _size; i++) newHeap[i] = _heap[i];
        _heap = newHeap;
    }
}
