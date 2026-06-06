namespace SkyNet.DSA.Graphs;

/// <summary>
/// Kruskal's Minimum Spanning Tree algorithm using Union-Find (Disjoint Set).
/// Reference: Kruskal, J.B. (1956) 'On the shortest spanning subtree of a graph',
///            Proceedings of the American Mathematical Society, 7(1), pp. 48–50.
/// Time complexity: O(E log E)
/// </summary>
public class KruskalMST
{
    public MSTResult Compute(AdjacencyListGraph graph)
    {
        var allEdges = graph.GetAllEdges();
        var vertices = graph.GetAllVertices();
        int v = vertices.Length;

        if (v == 0) return new MSTResult();

        // Sort edges by weight (MergeSort — O(E log E))
        SortEdges(allEdges, 0, allEdges.Length - 1);

        // Union-Find
        var parent = new int[v];
        var rank = new int[v];
        for (int i = 0; i < v; i++) parent[i] = i;

        var mstEdges = new MSTEdge[v - 1];
        int mstCount = 0;
        double totalWeight = 0;

        foreach (var edge in allEdges)
        {
            if (mstCount == v - 1) break;

            int fromIdx = IndexOf(vertices, edge.From);
            int toIdx = IndexOf(vertices, edge.To);
            if (fromIdx < 0 || toIdx < 0) continue;

            int rootFrom = Find(parent, fromIdx);
            int rootTo = Find(parent, toIdx);

            if (rootFrom != rootTo)
            {
                mstEdges[mstCount++] = new MSTEdge(edge.From, edge.To, edge.Weight);
                totalWeight += edge.Weight;
                Union(parent, rank, rootFrom, rootTo);
            }
        }

        return new MSTResult
        {
            Edges = mstEdges[..mstCount],
            TotalWeight = totalWeight,
            EdgeCount = mstCount
        };
    }

    private int Find(int[] parent, int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]]; // path compression
            x = parent[x];
        }
        return x;
    }

    private void Union(int[] parent, int[] rank, int x, int y)
    {
        if (rank[x] < rank[y]) (x, y) = (y, x);
        parent[y] = x;
        if (rank[x] == rank[y]) rank[x]++;
    }

    // MergeSort on edges by weight
    private void SortEdges(GraphEdge[] edges, int left, int right)
    {
        if (left >= right) return;
        int mid = (left + right) / 2;
        SortEdges(edges, left, mid);
        SortEdges(edges, mid + 1, right);
        Merge(edges, left, mid, right);
    }

    private void Merge(GraphEdge[] edges, int left, int mid, int right)
    {
        int n1 = mid - left + 1, n2 = right - mid;
        var L = new GraphEdge[n1];
        var R = new GraphEdge[n2];
        for (int i = 0; i < n1; i++) L[i] = edges[left + i];
        for (int j = 0; j < n2; j++) R[j] = edges[mid + 1 + j];

        int a = 0, b = 0, k = left;
        while (a < n1 && b < n2)
            edges[k++] = L[a].Weight <= R[b].Weight ? L[a++] : R[b++];
        while (a < n1) edges[k++] = L[a++];
        while (b < n2) edges[k++] = R[b++];
    }

    private static int IndexOf(string[] arr, string val)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return i;
        return -1;
    }
}

public class MSTResult
{
    public MSTEdge[] Edges { get; set; } = Array.Empty<MSTEdge>();
    public double TotalWeight { get; set; }
    public int EdgeCount { get; set; }
}

public class MSTEdge
{
    public string From { get; }
    public string To { get; }
    public double Weight { get; }
    public MSTEdge(string from, string to, double weight)
    { From = from; To = to; Weight = weight; }
}
