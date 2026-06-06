namespace SkyNet.DSA.Queues;

/// <summary>
/// Singly-Linked Stack — LIFO for cargo hold simulation.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 236–238.
/// Push: O(1) | Pop: O(1) | Peek: O(1)
/// </summary>
public class LinkedStack<T>
{
    private StackNode<T>? _top;
    private int _count;

    public int Count => _count;
    public bool IsEmpty => _top == null;

    public void Push(T item)
    {
        _top = new StackNode<T>(item, _top);
        _count++;
    }

    public T Pop()
    {
        if (IsEmpty) throw new InvalidOperationException("Stack is empty (cargo hold is empty).");
        var item = _top!.Value;
        _top = _top.Next;
        _count--;
        return item;
    }

    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("Stack is empty.");
        return _top!.Value;
    }

    public T[] ToArray()
    {
        var result = new T[_count];
        var node = _top;
        for (int i = 0; i < _count; i++)
        {
            result[i] = node!.Value;
            node = node.Next;
        }
        return result;
    }

    public void Clear()
    {
        _top = null;
        _count = 0;
    }
}

internal class StackNode<T>
{
    public T Value { get; }
    public StackNode<T>? Next { get; }

    public StackNode(T value, StackNode<T>? next)
    {
        Value = value;
        Next = next;
    }
}
