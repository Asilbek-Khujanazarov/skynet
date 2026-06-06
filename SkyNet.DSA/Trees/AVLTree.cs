namespace SkyNet.DSA.Trees;

/// <summary>
/// Self-balancing AVL Tree for flight price storage and range queries.
/// Reference: Adelson-Velsky, G. and Landis, E. (1962) 'An algorithm for the organization of information',
///            Soviet Mathematics Doklady, 3, pp. 1259–1263.
/// Insert/Search/Delete: O(log n) | Range Query: O(log n + k)
/// </summary>
public class AVLTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private AVLNode<TKey, TValue>? _root;
    private int _count;

    public int Count => _count;

    public void Insert(TKey key, TValue value)
    {
        _root = Insert(_root, key, value);
    }

    public TValue? Search(TKey key)
    {
        var node = FindNode(_root, key);
        return node != null ? node.Value : default;
    }

    public bool Contains(TKey key) => FindNode(_root, key) != null;

    public void Delete(TKey key)
    {
        _root = Delete(_root, key);
    }

    /// <summary>Returns all values with key in [minKey, maxKey].</summary>
    public TValue[] RangeQuery(TKey minKey, TKey maxKey)
    {
        var result = new TValue[_count];
        int count = 0;
        RangeQuery(_root, minKey, maxKey, result, ref count);
        return result[..count];
    }

    public TKey? FindMin()
    {
        if (_root == null) return default;
        var node = _root;
        while (node.Left != null) node = node.Left;
        return node.Key;
    }

    public TKey? FindMax()
    {
        if (_root == null) return default;
        var node = _root;
        while (node.Right != null) node = node.Right;
        return node.Key;
    }

    // ── Private helpers ──────────────────────────────────────────

    private AVLNode<TKey, TValue>? FindNode(AVLNode<TKey, TValue>? node, TKey key)
    {
        while (node != null)
        {
            int cmp = key.CompareTo(node.Key);
            if (cmp == 0) return node;
            node = cmp < 0 ? node.Left : node.Right;
        }
        return null;
    }

    private AVLNode<TKey, TValue> Insert(AVLNode<TKey, TValue>? node, TKey key, TValue value)
    {
        if (node == null) { _count++; return new AVLNode<TKey, TValue>(key, value); }

        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)       node.Left  = Insert(node.Left, key, value);
        else if (cmp > 0)  node.Right = Insert(node.Right, key, value);
        else               node.Value = value; // update

        return Balance(node);
    }

    private AVLNode<TKey, TValue>? Delete(AVLNode<TKey, TValue>? node, TKey key)
    {
        if (node == null) return null;

        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)      node.Left  = Delete(node.Left, key);
        else if (cmp > 0) node.Right = Delete(node.Right, key);
        else
        {
            _count--;
            if (node.Left == null) return node.Right;
            if (node.Right == null) return node.Left;
            // Replace with in-order successor
            var succ = node.Right;
            while (succ.Left != null) succ = succ.Left;
            node.Key = succ.Key;
            node.Value = succ.Value;
            node.Right = Delete(node.Right, succ.Key);
        }
        return Balance(node);
    }

    private void RangeQuery(AVLNode<TKey, TValue>? node, TKey min, TKey max, TValue[] result, ref int count)
    {
        if (node == null) return;
        if (node.Key.CompareTo(min) > 0) RangeQuery(node.Left, min, max, result, ref count);
        if (node.Key.CompareTo(min) >= 0 && node.Key.CompareTo(max) <= 0)
            result[count++] = node.Value;
        if (node.Key.CompareTo(max) < 0) RangeQuery(node.Right, min, max, result, ref count);
    }

    private AVLNode<TKey, TValue> Balance(AVLNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int bf = BalanceFactor(node);

        if (bf > 1)
        {
            if (BalanceFactor(node.Left!) < 0)
                node.Left = RotateLeft(node.Left!);
            return RotateRight(node);
        }
        if (bf < -1)
        {
            if (BalanceFactor(node.Right!) > 0)
                node.Right = RotateRight(node.Right!);
            return RotateLeft(node);
        }
        return node;
    }

    private AVLNode<TKey, TValue> RotateRight(AVLNode<TKey, TValue> y)
    {
        var x = y.Left!;
        y.Left = x.Right;
        x.Right = y;
        UpdateHeight(y);
        UpdateHeight(x);
        return x;
    }

    private AVLNode<TKey, TValue> RotateLeft(AVLNode<TKey, TValue> x)
    {
        var y = x.Right!;
        x.Right = y.Left;
        y.Left = x;
        UpdateHeight(x);
        UpdateHeight(y);
        return y;
    }

    private static void UpdateHeight(AVLNode<TKey, TValue> n)
        => n.Height = 1 + Math.Max(Height(n.Left), Height(n.Right));

    private static int Height(AVLNode<TKey, TValue>? n) => n?.Height ?? 0;
    private static int BalanceFactor(AVLNode<TKey, TValue> n) => Height(n.Left) - Height(n.Right);
}

internal class AVLNode<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public int Height { get; set; } = 1;
    public AVLNode<TKey, TValue>? Left { get; set; }
    public AVLNode<TKey, TValue>? Right { get; set; }

    public AVLNode(TKey key, TValue value) { Key = key; Value = value; }
}
