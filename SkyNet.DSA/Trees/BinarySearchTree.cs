namespace SkyNet.DSA.Trees;

/// <summary>
/// Binary Search Tree for flight indexing.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 312–323.
/// Search/Insert/Delete: O(log n) average, O(n) worst
/// </summary>
public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private BSTNode<TKey, TValue>? _root;
    public int Count { get; private set; }

    public void Insert(TKey key, TValue value)
    {
        _root = Insert(_root, key, value);
    }

    public TValue? Search(TKey key)
    {
        var node = _root;
        while (node != null)
        {
            int cmp = key.CompareTo(node.Key);
            if (cmp == 0) return node.Value;
            node = cmp < 0 ? node.Left : node.Right;
        }
        return default;
    }

    public bool Contains(TKey key) => Search(key) != null;

    public TKey? Min()
    {
        if (_root == null) return default;
        var n = _root;
        while (n.Left != null) n = n.Left;
        return n.Key;
    }

    public TKey? Max()
    {
        if (_root == null) return default;
        var n = _root;
        while (n.Right != null) n = n.Right;
        return n.Key;
    }

    /// <summary>In-order traversal — returns keys sorted ascending.</summary>
    public TKey[] InOrder()
    {
        var result = new TKey[Count];
        int idx = 0;
        InOrder(_root, result, ref idx);
        return result;
    }

    private void InOrder(BSTNode<TKey, TValue>? node, TKey[] result, ref int idx)
    {
        if (node == null) return;
        InOrder(node.Left, result, ref idx);
        result[idx++] = node.Key;
        InOrder(node.Right, result, ref idx);
    }

    private BSTNode<TKey, TValue> Insert(BSTNode<TKey, TValue>? node, TKey key, TValue value)
    {
        if (node == null) { Count++; return new BSTNode<TKey, TValue>(key, value); }
        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)      node.Left  = Insert(node.Left, key, value);
        else if (cmp > 0) node.Right = Insert(node.Right, key, value);
        else              node.Value = value;
        return node;
    }
}

internal class BSTNode<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public BSTNode<TKey, TValue>? Left { get; set; }
    public BSTNode<TKey, TValue>? Right { get; set; }
    public BSTNode(TKey key, TValue value) { Key = key; Value = value; }
}
