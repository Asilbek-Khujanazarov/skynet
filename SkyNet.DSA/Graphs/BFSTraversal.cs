namespace SkyNet.DSA.Graphs;

/// <summary>
/// Breadth-First Search traversal using a custom circular queue.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 554–558.
/// Time complexity: O(V + E)
/// </summary>
public class BFSTraversal
{
    private readonly AdjacencyListGraph _graph;

    public BFSTraversal(AdjacencyListGraph graph)
    {
        _graph = graph;
    }

    /// <summary>Returns all airports reachable from start in BFS order.</summary>
    public string[] Traverse(string start)
    {
        var vertices = _graph.GetAllVertices();
        int n = vertices.Length;

        bool[] visited = new bool[n];
        var result = new string[n];
        int resultCount = 0;

        int startIdx = IndexOf(vertices, start);
        if (startIdx < 0) return Array.Empty<string>();

        // Custom circular queue
        var queue = new int[n];
        int head = 0, tail = 0;

        visited[startIdx] = true;
        queue[tail++] = startIdx;

        while (head != tail)
        {
            int uIdx = queue[head++];
            result[resultCount++] = vertices[uIdx];

            var neighbors = _graph.GetNeighbors(vertices[uIdx]);
            foreach (var edge in neighbors)
            {
                int vIdx = IndexOf(vertices, edge.To);
                if (vIdx >= 0 && !visited[vIdx])
                {
                    visited[vIdx] = true;
                    queue[tail++] = vIdx;
                }
            }
        }

        return result[..resultCount];
    }

    /// <summary>Returns shortest hop-count path from source to target.</summary>
    public string[] ShortestHopPath(string source, string target)
    {
        var vertices = _graph.GetAllVertices();
        int n = vertices.Length;

        int srcIdx = IndexOf(vertices, source);
        int tgtIdx = IndexOf(vertices, target);
        if (srcIdx < 0 || tgtIdx < 0) return Array.Empty<string>();

        bool[] visited = new bool[n];
        int[] prev = new int[n];
        for (int i = 0; i < n; i++) prev[i] = -1;

        var queue = new int[n];
        int head = 0, tail = 0;

        visited[srcIdx] = true;
        queue[tail++] = srcIdx;

        while (head != tail)
        {
            int uIdx = queue[head++];
            if (uIdx == tgtIdx) break;

            var neighbors = _graph.GetNeighbors(vertices[uIdx]);
            foreach (var edge in neighbors)
            {
                int vIdx = IndexOf(vertices, edge.To);
                if (vIdx >= 0 && !visited[vIdx])
                {
                    visited[vIdx] = true;
                    prev[vIdx] = uIdx;
                    queue[tail++] = vIdx;
                }
            }
        }

        if (!visited[tgtIdx]) return Array.Empty<string>();

        // Reconstruct
        var path = new int[n];
        int len = 0;
        int cur = tgtIdx;
        while (cur != -1) { path[len++] = cur; cur = prev[cur]; }

        var result = new string[len];
        for (int i = 0; i < len; i++) result[i] = vertices[path[len - 1 - i]];
        return result;
    }

    private static int IndexOf(string[] arr, string val)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return i;
        return -1;
    }
}
