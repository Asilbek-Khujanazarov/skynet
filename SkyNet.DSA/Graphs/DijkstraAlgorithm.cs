namespace SkyNet.DSA.Graphs;

/// <summary>
/// Dijkstra's Shortest Path Algorithm using a custom Min-Heap priority queue.
/// Reference: Dijkstra, E.W. (1959) 'A note on two problems in connexion with graphs',
///            Numerische Mathematik, 1(1), pp. 269–271.
/// Time complexity: O((V + E) log V)
/// </summary>
public class DijkstraAlgorithm
{
    private readonly AdjacencyListGraph _graph;

    public DijkstraAlgorithm(AdjacencyListGraph graph)
    {
        _graph = graph;
    }

    public DijkstraResult FindShortestPath(string source, string target)
    {
        var vertices = _graph.GetAllVertices();
        int n = vertices.Length;

        if (n == 0) return DijkstraResult.NotFound();

        // Distance array (parallel to vertices)
        double[] dist = new double[n];
        string[] prev = new string[n];
        bool[] visited = new bool[n];

        for (int i = 0; i < n; i++)
        {
            dist[i] = double.MaxValue;
            prev[i] = string.Empty;
            visited[i] = false;
        }

        int srcIdx = IndexOf(vertices, source);
        int tgtIdx = IndexOf(vertices, target);

        if (srcIdx < 0 || tgtIdx < 0) return DijkstraResult.NotFound();

        dist[srcIdx] = 0;

        // Custom min-heap: (distance, vertexIndex)
        var heap = new DijkstraMinHeap(n * 2);
        heap.Insert(0.0, srcIdx);

        while (!heap.IsEmpty())
        {
            var (d, uIdx) = heap.ExtractMin();

            if (visited[uIdx]) continue;
            visited[uIdx] = true;

            if (uIdx == tgtIdx) break;

            var neighbors = _graph.GetNeighbors(vertices[uIdx]);
            foreach (var edge in neighbors)
            {
                int vIdx = IndexOf(vertices, edge.To);
                if (vIdx < 0 || visited[vIdx]) continue;

                double newDist = dist[uIdx] + edge.Weight;
                if (newDist < dist[vIdx])
                {
                    dist[vIdx] = newDist;
                    prev[vIdx] = vertices[uIdx];
                    heap.Insert(newDist, vIdx);
                }
            }
        }

        if (dist[tgtIdx] == double.MaxValue) return DijkstraResult.NotFound();

        // Reconstruct path
        var path = new string[n];
        int pathLen = 0;
        string current = target;
        while (current != string.Empty)
        {
            path[pathLen++] = current;
            int ci = IndexOf(vertices, current);
            current = prev[ci];
        }

        // Reverse path
        var finalPath = new string[pathLen];
        for (int i = 0; i < pathLen; i++)
            finalPath[i] = path[pathLen - 1 - i];

        return new DijkstraResult
        {
            Path = finalPath,
            TotalWeight = dist[tgtIdx],
            Found = true
        };
    }

    private static int IndexOf(string[] arr, string val)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return i;
        return -1;
    }
}

public class DijkstraResult
{
    public string[] Path { get; set; } = Array.Empty<string>();
    public double TotalWeight { get; set; }
    public bool Found { get; set; }

    public static DijkstraResult NotFound() => new() { Found = false };
}

// Internal min-heap for Dijkstra
internal class DijkstraMinHeap
{
    private (double dist, int idx)[] _heap;
    private int _size;

    public DijkstraMinHeap(int capacity)
    {
        _heap = new (double, int)[capacity];
        _size = 0;
    }

    public bool IsEmpty() => _size == 0;

    public void Insert(double dist, int idx)
    {
        if (_size >= _heap.Length) Resize();
        _heap[_size] = (dist, idx);
        BubbleUp(_size++);
    }

    public (double dist, int idx) ExtractMin()
    {
        var min = _heap[0];
        _heap[0] = _heap[--_size];
        BubbleDown(0);
        return min;
    }

    private void BubbleUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_heap[parent].dist <= _heap[i].dist) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void BubbleDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1, right = 2 * i + 2, smallest = i;
            if (left < _size && _heap[left].dist < _heap[smallest].dist) smallest = left;
            if (right < _size && _heap[right].dist < _heap[smallest].dist) smallest = right;
            if (smallest == i) break;
            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int a, int b) => (_heap[a], _heap[b]) = (_heap[b], _heap[a]);

    private void Resize()
    {
        var newHeap = new (double, int)[_heap.Length * 2];
        for (int i = 0; i < _size; i++) newHeap[i] = _heap[i];
        _heap = newHeap;
    }
}
