namespace SkyNet.DSA.Trees;

/// <summary>
/// Trie (Prefix Tree) for airport name AutoComplete.
/// Reference: Sedgewick, R. and Wayne, K. (2011) Algorithms, 4th ed. Addison-Wesley, pp. 730–745.
/// Insert: O(L) | Search: O(L) | AutoComplete: O(L + k)  where L = word length, k = results
/// </summary>
public class TrieTree
{
    private readonly TrieNode _root = new();
    private int _wordCount;

    public int WordCount => _wordCount;

    public void Insert(string word, string metadata = "")
    {
        if (string.IsNullOrEmpty(word)) return;
        var node = _root;
        foreach (char ch in word.ToUpperInvariant())
        {
            int idx = CharIndex(ch);
            if (idx < 0) continue;
            node.Children[idx] ??= new TrieNode();
            node = node.Children[idx]!;
        }
        if (!node.IsEnd) _wordCount++;
        node.IsEnd = true;
        node.Word = word;
        node.Metadata = metadata;
    }

    public bool Search(string word)
    {
        var node = FindNode(word);
        return node != null && node.IsEnd;
    }

    public bool StartsWith(string prefix)
        => FindNode(prefix) != null;

    /// <summary>Returns up to maxResults words starting with the given prefix.</summary>
    public TrieResult[] AutoComplete(string prefix, int maxResults = 10)
    {
        var results = new TrieResult[maxResults];
        int count = 0;

        var node = FindNode(prefix);
        if (node == null) return Array.Empty<TrieResult>();

        CollectWords(node, results, ref count, maxResults);
        return results[..count];
    }

    private TrieNode? FindNode(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return _root;
        var node = _root;
        foreach (char ch in prefix.ToUpperInvariant())
        {
            int idx = CharIndex(ch);
            if (idx < 0 || node.Children[idx] == null) return null;
            node = node.Children[idx]!;
        }
        return node;
    }

    private void CollectWords(TrieNode node, TrieResult[] results, ref int count, int max)
    {
        if (count >= max) return;
        if (node.IsEnd)
            results[count++] = new TrieResult(node.Word!, node.Metadata);

        for (int i = 0; i < 38; i++)
            if (node.Children[i] != null)
                CollectWords(node.Children[i]!, results, ref count, max);
    }

    // A-Z (26) + 0-9 (10) + space (1) + hyphen (1) = 38
    private static int CharIndex(char c)
    {
        if (c >= 'A' && c <= 'Z') return c - 'A';
        if (c >= '0' && c <= '9') return 26 + (c - '0');
        if (c == ' ') return 36;
        if (c == '-') return 37;
        return -1;
    }
}

internal class TrieNode
{
    public TrieNode?[] Children { get; } = new TrieNode[38];
    public bool IsEnd { get; set; }
    public string? Word { get; set; }
    public string Metadata { get; set; } = string.Empty;
}

public class TrieResult
{
    public string Word { get; }
    public string Metadata { get; }
    public TrieResult(string word, string metadata) { Word = word; Metadata = metadata; }
}
