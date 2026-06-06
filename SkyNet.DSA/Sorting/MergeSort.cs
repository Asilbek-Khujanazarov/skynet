namespace SkyNet.DSA.Sorting;

/// <summary>
/// Generic MergeSort — stable, O(n log n) guaranteed.
/// Used for sorting by fuel efficiency (stable preserves original order on ties).
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 36–42.
/// Time: O(n log n) | Space: O(n)
/// </summary>
public static class MergeSort
{
    public static void Sort<T>(T[] arr, Comparison<T> compare)
    {
        if (arr.Length <= 1) return;
        var temp = new T[arr.Length];
        Sort(arr, temp, 0, arr.Length - 1, compare);
    }

    private static void Sort<T>(T[] arr, T[] temp, int left, int right, Comparison<T> compare)
    {
        if (left >= right) return;
        int mid = (left + right) / 2;
        Sort(arr, temp, left, mid, compare);
        Sort(arr, temp, mid + 1, right, compare);
        Merge(arr, temp, left, mid, right, compare);
    }

    private static void Merge<T>(T[] arr, T[] temp, int left, int mid, int right, Comparison<T> compare)
    {
        for (int k = left; k <= right; k++) temp[k] = arr[k];

        int i = left, j = mid + 1;
        for (int k = left; k <= right; k++)
        {
            if (i > mid)                             arr[k] = temp[j++];
            else if (j > right)                      arr[k] = temp[i++];
            else if (compare(temp[i], temp[j]) <= 0) arr[k] = temp[i++];
            else                                     arr[k] = temp[j++];
        }
    }
}
