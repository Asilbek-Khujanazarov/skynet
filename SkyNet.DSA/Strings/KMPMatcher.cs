namespace SkyNet.DSA.Strings;

/// <summary>
/// Knuth-Morris-Pratt string matching algorithm.
/// Used to search passenger names in large manifests.
/// Reference: Knuth, D.E., Morris, J.H. and Pratt, V.R. (1977)
///            'Fast pattern matching in strings', SIAM Journal on Computing, 6(2), pp. 323–350.
/// Time: O(n + m) | Space: O(m)  where n = text length, m = pattern length
/// </summary>
public class KMPMatcher
{
    /// <summary>
    /// Returns all start indices where pattern appears in text (case-insensitive).
    /// </summary>
    public int[] Search(string text, string pattern)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return Array.Empty<int>();

        string t = text.ToUpperInvariant();
        string p = pattern.ToUpperInvariant();

        int n = t.Length, m = p.Length;
        int[] lps = BuildLPS(p);
        int[] matches = new int[n];
        int matchCount = 0;

        int i = 0, j = 0;
        while (i < n)
        {
            if (t[i] == p[j])
            {
                i++; j++;
                if (j == m)
                {
                    matches[matchCount++] = i - j;
                    j = lps[j - 1];
                }
            }
            else if (j > 0)
                j = lps[j - 1];
            else
                i++;
        }

        return matches[..matchCount];
    }

    /// <summary>Returns true if pattern exists in text.</summary>
    public bool Contains(string text, string pattern)
        => Search(text, pattern).Length > 0;

    /// <summary>
    /// Searches passenger names: returns indices of matching entries.
    /// </summary>
    public int[] SearchInList(string[] names, string pattern)
    {
        var results = new int[names.Length];
        int count = 0;
        for (int i = 0; i < names.Length; i++)
            if (Contains(names[i], pattern))
                results[count++] = i;
        return results[..count];
    }

    // Builds the Longest Proper Prefix-Suffix array
    private static int[] BuildLPS(string pattern)
    {
        int m = pattern.Length;
        int[] lps = new int[m];
        int len = 0, i = 1;

        while (i < m)
        {
            if (pattern[i] == pattern[len])
                lps[i++] = ++len;
            else if (len > 0)
                len = lps[len - 1];
            else
                lps[i++] = 0;
        }
        return lps;
    }
}
