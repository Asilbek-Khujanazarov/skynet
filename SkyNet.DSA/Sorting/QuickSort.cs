namespace SkyNet.DSA.Sorting;

/// <summary>
/// Generic QuickSort with median-of-three pivot selection.
/// Reference: Hoare, C.A.R. (1962) 'Quicksort', Computer Journal, 5(1), pp. 10–16.
/// Time: O(n log n) average, O(n²) worst | Space: O(log n) stack
/// </summary>
public static class QuickSort
{
    public static void Sort<T>(T[] arr, Comparison<T> compare)
    {
        if (arr.Length <= 1) return;
        Sort(arr, 0, arr.Length - 1, compare);
    }

    private static void Sort<T>(T[] arr, int low, int high, Comparison<T> compare)
    {
        if (low >= high) return;

        // Insertion sort for small subarrays (optimisation)
        if (high - low < 10)
        {
            InsertionSort(arr, low, high, compare);
            return;
        }

        int pivot = Partition(arr, low, high, compare);
        Sort(arr, low, pivot - 1, compare);
        Sort(arr, pivot + 1, high, compare);
    }

    private static int Partition<T>(T[] arr, int low, int high, Comparison<T> compare)
    {
        // Median-of-three pivot
        int mid = (low + high) / 2;
        if (compare(arr[low], arr[mid]) > 0) Swap(arr, low, mid);
        if (compare(arr[low], arr[high]) > 0) Swap(arr, low, high);
        if (compare(arr[mid], arr[high]) > 0) Swap(arr, mid, high);

        // Move pivot to high-1
        Swap(arr, mid, high);
        T pivot = arr[high];

        int i = low - 1;
        for (int j = low; j < high; j++)
            if (compare(arr[j], pivot) <= 0)
                Swap(arr, ++i, j);

        Swap(arr, i + 1, high);
        return i + 1;
    }

    private static void InsertionSort<T>(T[] arr, int low, int high, Comparison<T> compare)
    {
        for (int i = low + 1; i <= high; i++)
        {
            T key = arr[i];
            int j = i - 1;
            while (j >= low && compare(arr[j], key) > 0)
            { arr[j + 1] = arr[j]; j--; }
            arr[j + 1] = key;
        }
    }

    private static void Swap<T>(T[] arr, int a, int b)
        => (arr[a], arr[b]) = (arr[b], arr[a]);
}
