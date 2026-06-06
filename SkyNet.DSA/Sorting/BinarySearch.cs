namespace SkyNet.DSA.Sorting;

/// <summary>
/// Binary Search on sorted arrays — O(log n).
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 798–799.
/// </summary>
public static class BinarySearch
{
    /// <summary>Returns index of target, or -1 if not found. Array must be sorted.</summary>
    public static int Search<T>(T[] arr, T target, Comparison<T> compare)
    {
        int low = 0, high = arr.Length - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int cmp = compare(arr[mid], target);
            if (cmp == 0) return mid;
            if (cmp < 0) low = mid + 1;
            else         high = mid - 1;
        }
        return -1;
    }

    /// <summary>Returns first index where condition is true (lower bound).</summary>
    public static int LowerBound<T>(T[] arr, T target, Comparison<T> compare)
    {
        int low = 0, high = arr.Length;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (compare(arr[mid], target) < 0) low = mid + 1;
            else high = mid;
        }
        return low;
    }

    /// <summary>Returns first index where value > target (upper bound).</summary>
    public static int UpperBound<T>(T[] arr, T target, Comparison<T> compare)
    {
        int low = 0, high = arr.Length;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (compare(arr[mid], target) <= 0) low = mid + 1;
            else high = mid;
        }
        return low;
    }
}
