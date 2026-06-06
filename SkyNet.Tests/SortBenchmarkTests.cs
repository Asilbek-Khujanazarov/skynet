using SkyNet.DSA.Hashing;
using SkyNet.DSA.Sorting;
using SkyNet.DSA.Strings;
using SkyNet.DSA.Trees;
using Xunit;

namespace SkyNet.Tests;

public class SortBenchmarkTests
{
    [Fact]
    public void QuickSort_SortsIntegerArray_Ascending()
    {
        int[] arr = { 5, 3, 8, 1, 9, 2, 7, 4, 6 };
        QuickSort.Sort(arr, (a, b) => a.CompareTo(b));

        for (int i = 1; i < arr.Length; i++)
            Assert.True(arr[i - 1] <= arr[i]);
    }

    [Fact]
    public void MergeSort_SortsStringArray_Alphabetically()
    {
        string[] arr = { "LHR", "TAS", "DXB", "JFK", "AMS" };
        MergeSort.Sort(arr, string.Compare);

        for (int i = 1; i < arr.Length; i++)
            Assert.True(string.Compare(arr[i - 1], arr[i]) <= 0);
    }

    [Fact]
    public void BinarySearch_FindsElement_ReturnsCorrectIndex()
    {
        int[] sorted = { 1, 3, 5, 7, 9, 11, 13 };
        int idx = BinarySearch.Search(sorted, 7, (a, b) => a.CompareTo(b));

        Assert.Equal(3, idx);
    }

    [Fact]
    public void BinarySearch_MissingElement_ReturnsNegative()
    {
        int[] sorted = { 2, 4, 6, 8, 10 };
        int idx = BinarySearch.Search(sorted, 5, (a, b) => a.CompareTo(b));

        Assert.Equal(-1, idx);
    }

    [Fact]
    public void AVLTree_InsertAndSearch_ReturnsCorrectValue()
    {
        var tree = new AVLTree<double, string>();
        tree.Insert(150.0, "SK0010");
        tree.Insert(300.0, "SK0020");
        tree.Insert(450.0, "SK0030");
        tree.Insert(100.0, "SK0005");

        Assert.Equal("SK0020", tree.Search(300.0));
        Assert.Equal("SK0005", tree.Search(100.0));
    }

    [Fact]
    public void AVLTree_RangeQuery_ReturnsInRangeOnly()
    {
        var tree = new AVLTree<double, string>();
        tree.Insert(100.0, "A");
        tree.Insert(200.0, "B");
        tree.Insert(300.0, "C");
        tree.Insert(400.0, "D");
        tree.Insert(500.0, "E");

        var results = tree.RangeQuery(150.0, 350.0);

        Assert.Equal(2, results.Length);
        Assert.Contains("B", results);
        Assert.Contains("C", results);
    }

    [Fact]
    public void HashTable_SetAndGet_ReturnsValue()
    {
        var ht = new HashTable<string, string>();
        ht.Set("SKY-001", "John Smith");
        ht.Set("SKY-002", "Jane Doe");

        Assert.Equal("John Smith", ht.Get("SKY-001"));
        Assert.Equal("Jane Doe",   ht.Get("SKY-002"));
    }

    [Fact]
    public void HashTable_Remove_DeletesEntry()
    {
        var ht = new HashTable<string, int>();
        ht.Set("A", 1);
        ht.Set("B", 2);
        ht.Remove("A");

        Assert.False(ht.ContainsKey("A"));
        Assert.True(ht.ContainsKey("B"));
    }

    [Fact]
    public void KMPMatcher_FindsPattern_ReturnsCorrectIndices()
    {
        var kmp     = new KMPMatcher();
        var indices = kmp.Search("TASHKENT INTERNATIONAL AIRPORT", "NATIONAL");

        Assert.Single(indices);
        Assert.Equal(12, indices[0]);
    }

    [Fact]
    public void KMPMatcher_NotFound_ReturnsEmpty()
    {
        var kmp     = new KMPMatcher();
        var indices = kmp.Search("HELLO WORLD", "XYZ");

        Assert.Empty(indices);
    }

    [Fact]
    public void TrieTree_AutoComplete_ReturnsPrefixMatches()
    {
        var trie = new TrieTree();
        trie.Insert("Tashkent", "TAS");
        trie.Insert("Tokyo",    "NRT");
        trie.Insert("Toronto",  "YYZ");
        trie.Insert("Tbilisi",  "TBS");

        var results = trie.AutoComplete("To", 10);

        Assert.Equal(2, results.Length);
        Assert.Contains(results, r => r.Word == "Tokyo");
        Assert.Contains(results, r => r.Word == "Toronto");
    }

    [Fact]
    public void LRUCache_Eviction_RemovesLRU()
    {
        var cache = new LRUCache<string, int>(3);
        cache.Put("A", 1);
        cache.Put("B", 2);
        cache.Put("C", 3);

        // Access A to make it recently used
        cache.TryGet("A", out _);

        // Insert D — should evict B (LRU)
        cache.Put("D", 4);

        Assert.False(cache.ContainsKey("B"));
        Assert.True(cache.ContainsKey("A"));
        Assert.True(cache.ContainsKey("D"));
    }
}
