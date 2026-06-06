namespace SkyNet.DSA.Hashing;

/// <summary>
/// Least Recently Used Cache using a Hash Table + Doubly Linked List.
/// Caches frequently searched routes for O(1) get/put.
/// Reference: Sedgewick, R. and Wayne, K. (2011) Algorithms, 4th ed. Addison-Wesley.
/// Get: O(1) | Put: O(1) | Space: O(capacity)
/// </summary>
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly HashTable<TKey, LRUNode<TKey, TValue>> _map;
    private LRUNode<TKey, TValue> _head; // most recent
    private LRUNode<TKey, TValue> _tail; // least recent
    private int _count;

    public int Count => _count;
    public int Capacity => _capacity;
    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _map = new HashTable<TKey, LRUNode<TKey, TValue>>(capacity * 2);

        // Sentinel nodes
        _head = new LRUNode<TKey, TValue>(default!, default!);
        _tail = new LRUNode<TKey, TValue>(default!, default!);
        _head.Next = _tail;
        _tail.Prev = _head;
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        if (_map.TryGet(key, out var node) && node != null)
        {
            MoveToFront(node);
            Hits++;
            value = node.Value;
            return true;
        }
        Misses++;
        value = default;
        return false;
    }

    public void Put(TKey key, TValue value)
    {
        if (_map.TryGet(key, out var existing) && existing != null)
        {
            existing.Value = value;
            MoveToFront(existing);
            return;
        }

        if (_count >= _capacity) Evict();

        var node = new LRUNode<TKey, TValue>(key, value);
        InsertFront(node);
        _map.Set(key, node);
        _count++;
    }

    public bool ContainsKey(TKey key) => _map.ContainsKey(key);

    private void MoveToFront(LRUNode<TKey, TValue> node)
    {
        Remove(node);
        InsertFront(node);
    }

    private void InsertFront(LRUNode<TKey, TValue> node)
    {
        node.Next = _head.Next;
        node.Prev = _head;
        _head.Next!.Prev = node;
        _head.Next = node;
    }

    private void Remove(LRUNode<TKey, TValue> node)
    {
        node.Prev!.Next = node.Next;
        node.Next!.Prev = node.Prev;
    }

    private void Evict()
    {
        var lru = _tail.Prev!;
        if (lru == _head) return;
        Remove(lru);
        _map.Remove(lru.Key);
        _count--;
    }
}

internal class LRUNode<TKey, TValue>
{
    public TKey Key { get; }
    public TValue Value { get; set; }
    public LRUNode<TKey, TValue>? Prev { get; set; }
    public LRUNode<TKey, TValue>? Next { get; set; }

    public LRUNode(TKey key, TValue value) { Key = key; Value = value; }
}
