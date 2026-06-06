namespace SkyNet.DSA.Graphs;

/// <summary>
/// Generic weighted directed graph using Adjacency List representation.
/// No System.Collections.Generic used — all custom data structures.
/// Reference: Cormen et al. (2022) Introduction to Algorithms, 4th ed., MIT Press.
/// Space complexity: O(V + E)
/// </summary>
public class AdjacencyListGraph
{
    // Custom simple dictionary via parallel arrays
    private readonly string[] _vertices;
    private readonly GraphEdge[][] _adjacency;
    private readonly int[] _edgeCounts;
    private int _vertexCount;
    private readonly int _maxVertices;
    private readonly int _maxEdgesPerVertex;

    public int VertexCount => _vertexCount;

    public AdjacencyListGraph(int maxVertices = 2000, int maxEdgesPerVertex = 500)
    {
        _maxVertices = maxVertices;
        _maxEdgesPerVertex = maxEdgesPerVertex;
        _vertices = new string[maxVertices];
        _adjacency = new GraphEdge[maxVertices][];
        _edgeCounts = new int[maxVertices];

        for (int i = 0; i < maxVertices; i++)
        {
            _adjacency[i] = new GraphEdge[maxEdgesPerVertex];
            _edgeCounts[i] = 0;
        }
        _vertexCount = 0;
    }

    // O(V) — find vertex index
    private int IndexOf(string vertex)
    {
        for (int i = 0; i < _vertexCount; i++)
            if (_vertices[i] == vertex) return i;
        return -1;
    }

    // O(1) amortized
    public void AddVertex(string vertex)
    {
        if (IndexOf(vertex) >= 0) return;
        if (_vertexCount >= _maxVertices)
            throw new InvalidOperationException("Graph is full.");
        _vertices[_vertexCount++] = vertex;
    }

    // O(V) — adds directed weighted edge
    public void AddEdge(string from, string to, double weight)
    {
        int fi = IndexOf(from);
        int ti = IndexOf(to);
        if (fi < 0 || ti < 0) return;

        // Avoid duplicate edges
        for (int i = 0; i < _edgeCounts[fi]; i++)
            if (_adjacency[fi][i].To == to) return;

        if (_edgeCounts[fi] >= _maxEdgesPerVertex) return;

        _adjacency[fi][_edgeCounts[fi]++] = new GraphEdge(to, weight);
    }

    // Add undirected edge (both directions)
    public void AddUndirectedEdge(string from, string to, double weight)
    {
        AddEdge(from, to, weight);
        AddEdge(to, from, weight);
    }

    public GraphEdge[] GetNeighbors(string vertex)
    {
        int idx = IndexOf(vertex);
        if (idx < 0) return Array.Empty<GraphEdge>();

        var result = new GraphEdge[_edgeCounts[idx]];
        for (int i = 0; i < _edgeCounts[idx]; i++)
            result[i] = _adjacency[idx][i];
        return result;
    }

    public string[] GetAllVertices()
    {
        var result = new string[_vertexCount];
        for (int i = 0; i < _vertexCount; i++)
            result[i] = _vertices[i];
        return result;
    }

    public bool HasVertex(string vertex) => IndexOf(vertex) >= 0;

    public void RemoveVertex(string vertex)
    {
        int idx = IndexOf(vertex);
        if (idx < 0) return;
        // Shift vertices down
        for (int i = idx; i < _vertexCount - 1; i++)
        {
            _vertices[i] = _vertices[i + 1];
            _adjacency[i] = _adjacency[i + 1];
            _edgeCounts[i] = _edgeCounts[i + 1];
        }
        _vertexCount--;
        // Remove edges pointing to this vertex
        for (int i = 0; i < _vertexCount; i++)
        {
            int newCount = 0;
            for (int j = 0; j < _edgeCounts[i]; j++)
                if (_adjacency[i][j].To != vertex)
                    _adjacency[i][newCount++] = _adjacency[i][j];
            _edgeCounts[i] = newCount;
        }
    }

    // Returns all edges as flat list (for Kruskal MST)
    public GraphEdge[] GetAllEdges()
    {
        int total = 0;
        for (int i = 0; i < _vertexCount; i++) total += _edgeCounts[i];

        var edges = new GraphEdge[total];
        int idx = 0;
        for (int i = 0; i < _vertexCount; i++)
            for (int j = 0; j < _edgeCounts[i]; j++)
            {
                edges[idx] = new GraphEdge(_vertices[i], _adjacency[i][j].To, _adjacency[i][j].Weight);
                idx++;
            }
        return edges;
    }
}

public struct GraphEdge
{
    public string From { get; }
    public string To { get; }
    public double Weight { get; }

    public GraphEdge(string to, double weight) { From = string.Empty; To = to; Weight = weight; }
    public GraphEdge(string from, string to, double weight) { From = from; To = to; Weight = weight; }
}
