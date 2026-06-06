namespace SkyNet.DSA.Hashing;

/// <summary>
/// Generic Hash Table with separate chaining collision resolution.
/// Maps PNR/PassportId → Passenger profile in O(1) average.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 272–285.
/// Insert/Search/Delete: O(1) average, O(n) worst
/// </summary>
public class HashTable<TKey, TValue> where TKey : notnull
{
    private HashEntry<TKey, TValue>?[] _buckets;
    private int _capacity;
    private int _count;
    private const double LoadFactorThreshold = 0.75;

    public int Count => _count;
    public int Capacity => _capacity;

    public HashTable(int initialCapacity = 128)
    {
        _capacity = NextPrime(initialCapacity);
        _buckets = new HashEntry<TKey, TValue>[_capacity];
        _count = 0;
    }

    public void Set(TKey key, TValue value)
    {
        if ((double)_count / _capacity > LoadFactorThreshold) Rehash();

        int idx = GetBucketIndex(key);
        var entry = _buckets[idx];

        while (entry != null)
        {
            if (entry.Key.Equals(key)) { entry.Value = value; return; }
            entry = entry.Next;
        }

        // Prepend new entry (O(1))
        _buckets[idx] = new HashEntry<TKey, TValue>(key, value, _buckets[idx]);
        _count++;
    }

    public TValue? Get(TKey key)
    {
        int idx = GetBucketIndex(key);
        var entry = _buckets[idx];
        while (entry != null)
        {
            if (entry.Key.Equals(key)) return entry.Value;
            entry = entry.Next;
        }
        return default;
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        var result = Get(key);
        if (result != null) { value = result; return true; }
        value = default;
        return false;
    }

    public bool ContainsKey(TKey key) => Get(key) != null;

    public bool Remove(TKey key)
    {
        int idx = GetBucketIndex(key);
        var entry = _buckets[idx];
        HashEntry<TKey, TValue>? prev = null;

        while (entry != null)
        {
            if (entry.Key.Equals(key))
            {
                if (prev == null) _buckets[idx] = entry.Next;
                else prev.Next = entry.Next;
                _count--;
                return true;
            }
            prev = entry;
            entry = entry.Next;
        }
        return false;
    }

    public TKey[] GetAllKeys()
    {
        var keys = new TKey[_count];
        int idx = 0;
        foreach (var bucket in _buckets)
        {
            var entry = bucket;
            while (entry != null) { keys[idx++] = entry.Key; entry = entry.Next; }
        }
        return keys;
    }

    public TValue[] GetAllValues()
    {
        var values = new TValue[_count];
        int idx = 0;
        foreach (var bucket in _buckets)
        {
            var entry = bucket;
            while (entry != null) { values[idx++] = entry.Value; entry = entry.Next; }
        }
        return values;
    }

    // ── Internals ────────────────────────────────────────────────

    private int GetBucketIndex(TKey key)
        => Math.Abs(key.GetHashCode()) % _capacity;

    private void Rehash()
    {
        int newCapacity = NextPrime(_capacity * 2);
        var newBuckets = new HashEntry<TKey, TValue>[newCapacity];

        foreach (var bucket in _buckets)
        {
            var entry = bucket;
            while (entry != null)
            {
                int newIdx = Math.Abs(entry.Key.GetHashCode()) % newCapacity;
                var next = entry.Next;
                entry.Next = newBuckets[newIdx];
                newBuckets[newIdx] = entry;
                entry = next;
            }
        }

        _buckets = newBuckets;
        _capacity = newCapacity;
    }

    private static int NextPrime(int n)
    {
        if (n < 2) return 2;
        while (!IsPrime(n)) n++;
        return n;
    }

    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; (long)i * i <= n; i++)
            if (n % i == 0) return false;
        return true;
    }
}

internal class HashEntry<TKey, TValue>
{
    public TKey Key { get; }
    public TValue Value { get; set; }
    public HashEntry<TKey, TValue>? Next { get; set; }

    public HashEntry(TKey key, TValue value, HashEntry<TKey, TValue>? next)
    {
        Key = key; Value = value; Next = next;
    }
}
