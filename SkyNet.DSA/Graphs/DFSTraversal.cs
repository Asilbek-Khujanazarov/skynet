namespace SkyNet.DSA.Graphs;

/// <summary>
/// Depth-First Search traversal using a custom linked stack.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press, pp. 563–570.
/// Time complexity: O(V + E)
/// </summary>
public class DFSTraversal
{
    private readonly AdjacencyListGraph _graph;

    public DFSTraversal(AdjacencyListGraph graph)
    {
        _graph = graph;
    }

    /// <summary>Iterative DFS — returns all reachable vertices from start.</summary>
    public string[] Traverse(string start)
    {
        var vertices = _graph.GetAllVertices();
        int n = vertices.Length;

        bool[] visited = new bool[n];
        var result = new string[n];
        int resultCount = 0;

        int startIdx = IndexOf(vertices, start);
        if (startIdx < 0) return Array.Empty<string>();

        // Custom stack
        var stack = new int[n];
        int top = 0;
        stack[top++] = startIdx;

        while (top > 0)
        {
            int uIdx = stack[--top];
            if (visited[uIdx]) continue;

            visited[uIdx] = true;
            result[resultCount++] = vertices[uIdx];

            var neighbors = _graph.GetNeighbors(vertices[uIdx]);
            for (int i = neighbors.Length - 1; i >= 0; i--)
            {
                int vIdx = IndexOf(vertices, neighbors[i].To);
                if (vIdx >= 0 && !visited[vIdx])
                    stack[top++] = vIdx;
            }
        }

        return result[..resultCount];
    }

    /// <summary>Recursive DFS — visits all vertices, returns visit order.</summary>
    public string[] TraverseAll()
    {
        var vertices = _graph.GetAllVertices();
        int n = vertices.Length;
        bool[] visited = new bool[n];
        var result = new string[n];
        int count = 0;

        for (int i = 0; i < n; i++)
            if (!visited[i])
                DFSVisit(vertices, visited, i, result, ref count);

        return result[..count];
    }

    private void DFSVisit(string[] vertices, bool[] visited, int uIdx, string[] result, ref int count)
    {
        visited[uIdx] = true;
        result[count++] = vertices[uIdx];

        var neighbors = _graph.GetNeighbors(vertices[uIdx]);
        foreach (var edge in neighbors)
        {
            int vIdx = IndexOf(vertices, edge.To);
            if (vIdx >= 0 && !visited[vIdx])
                DFSVisit(vertices, visited, vIdx, result, ref count);
        }
    }

    private static int IndexOf(string[] arr, string val)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return i;
        return -1;
    }
}
