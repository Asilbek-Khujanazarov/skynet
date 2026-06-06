using System.Diagnostics;

namespace SkyNet.DSA.Sorting;

/// <summary>
/// Benchmarks QuickSort vs MergeSort on the same dataset.
/// Used in analytics to demonstrate performance comparison.
/// </summary>
public class SortBenchmark
{
    public BenchmarkResult Run<T>(T[] data, Comparison<T> compare)
    {
        // QuickSort
        var qs = new T[data.Length];
        data.CopyTo(qs, 0);
        var sw = Stopwatch.StartNew();
        QuickSort.Sort(qs, compare);
        sw.Stop();
        long qsMs = sw.ElapsedMilliseconds;

        // MergeSort
        var ms = new T[data.Length];
        data.CopyTo(ms, 0);
        sw.Restart();
        MergeSort.Sort(ms, compare);
        sw.Stop();
        long msMs = sw.ElapsedMilliseconds;

        return new BenchmarkResult
        {
            DataSize = data.Length,
            QuickSortMs = qsMs,
            MergeSortMs = msMs,
            QuickSortResult = qs,
            MergeSortResult = ms,
            FasterAlgorithm = qsMs <= msMs ? "QuickSort" : "MergeSort"
        };
    }
}

public class BenchmarkResult
{
    public int DataSize { get; set; }
    public long QuickSortMs { get; set; }
    public long MergeSortMs { get; set; }
    public object? QuickSortResult { get; set; }
    public object? MergeSortResult { get; set; }
    public string FasterAlgorithm { get; set; } = string.Empty;
}
