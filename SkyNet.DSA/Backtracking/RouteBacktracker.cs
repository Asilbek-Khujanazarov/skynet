namespace SkyNet.DSA.Backtracking;

/// <summary>
/// Recursive backtracking to find all alternative routes
/// when a primary hub airport is closed/unavailable.
/// Reference: Leiss, E. (2007) A Programmer's Companion to Algorithm Analysis.
///            Chapman and Hall/CRC, pp. 201–215.
/// Time: O(V!) worst case | Practical: bounded by maxDepth
/// </summary>
public class RouteBacktracker
{
    private readonly string[][] _adjacency;
    private readonly double[][] _weights;
    private readonly string[] _vertices;
    private readonly Dictionary<string, int> _indexMap;
    private readonly int _vertexCount;
    private readonly int _maxDepth;

    public RouteBacktracker(string[] vertices, string[][] adjacency, double[][] weights,
        Dictionary<string, int> indexMap, int maxDepth = 4)
    {
        _vertices    = vertices;
        _vertexCount = vertices.Length;
        _adjacency   = adjacency;
        _weights     = weights;
        _indexMap    = indexMap;
        _maxDepth    = maxDepth;
    }

    /// <summary>
    /// Returns all alternative routes from source to target avoiding closedAirport.
    /// Results are sorted by total cost.
    /// </summary>
    public BacktrackResult FindAlternativeRoutes(string source, string target, string closedAirport)
    {
        var allPaths = new string[200][];
        var allCosts = new double[200];
        int pathCount = 0;

        if (!_indexMap.TryGetValue(source, out int srcIdx) ||
            !_indexMap.TryGetValue(target, out int tgtIdx))
            return new BacktrackResult { Paths = Array.Empty<string[]>() };

        bool[] visited = new bool[_vertexCount];
        string[] currentPath = new string[_maxDepth + 1];
        currentPath[0] = source;
        visited[srcIdx] = true;

        Backtrack(srcIdx, tgtIdx, closedAirport, visited, currentPath, 1,
                  allPaths, allCosts, ref pathCount, 0.0);

        // Sort by cost (bubble sort — small result set)
        for (int i = 0; i < pathCount - 1; i++)
            for (int j = 0; j < pathCount - i - 1; j++)
                if (allCosts[j] > allCosts[j + 1])
                {
                    (allCosts[j], allCosts[j + 1]) = (allCosts[j + 1], allCosts[j]);
                    (allPaths[j], allPaths[j + 1]) = (allPaths[j + 1], allPaths[j]);
                }

        return new BacktrackResult
        {
            Paths = allPaths[..pathCount],
            Costs = allCosts[..pathCount],
            TotalFound = pathCount
        };
    }

    private void Backtrack(int current, int target, string closed, bool[] visited,
        string[] path, int depth, string[][] results, double[] costs,
        ref int count, double currentCost)
    {
        if (current == target)
        {
            if (count >= results.Length) return;
            var pathCopy = new string[depth];
            for (int i = 0; i < depth; i++) pathCopy[i] = path[i];
            results[count] = pathCopy;
            costs[count]   = currentCost;
            count++;
            return;
        }

        if (depth > _maxDepth) return;

        var neighbors = _adjacency[current];
        var weights   = _weights[current];

        for (int i = 0; i < neighbors.Length; i++)
        {
            string neighbor = neighbors[i];
            if (neighbor == closed) continue;

            if (!_indexMap.TryGetValue(neighbor, out int nIdx)) continue;
            if (visited[nIdx]) continue;

            visited[nIdx] = true;
            path[depth]   = neighbor;

            Backtrack(nIdx, target, closed, visited, path, depth + 1,
                      results, costs, ref count, currentCost + weights[i]);

            visited[nIdx] = false;
        }
    }
}

public class BacktrackResult
{
    public string[][] Paths { get; set; } = Array.Empty<string[]>();
    public double[] Costs { get; set; } = Array.Empty<double>();
    public int TotalFound { get; set; }
}
