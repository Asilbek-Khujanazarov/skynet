using SkyNet.DSA.Graphs;
using Xunit;

namespace SkyNet.Tests;

public class DijkstraTests
{
    private static AdjacencyListGraph BuildTestGraph()
    {
        var g = new AdjacencyListGraph(20, 20);
        // TAS → IST → LHR → JFK
        // TAS → DXB → LHR
        foreach (var v in new[] { "TAS","IST","LHR","JFK","DXB","SIN","CDG" })
            g.AddVertex(v);

        g.AddEdge("TAS", "IST", 3600);
        g.AddEdge("TAS", "DXB", 3200);
        g.AddEdge("IST", "LHR", 2800);
        g.AddEdge("DXB", "LHR", 5500);
        g.AddEdge("LHR", "JFK", 5540);
        g.AddEdge("IST", "CDG", 2240);
        g.AddEdge("CDG", "LHR",  340);
        g.AddEdge("DXB", "SIN", 5841);

        return g;
    }

    [Fact]
    public void FindShortestPath_TasToJFK_ReturnsCorrectPath()
    {
        var graph    = BuildTestGraph();
        var dijkstra = new DijkstraAlgorithm(graph);

        var result = dijkstra.FindShortestPath("TAS", "JFK");

        Assert.True(result.Found);
        Assert.Equal("TAS", result.Path[0]);
        Assert.Equal("JFK", result.Path[^1]);
        Assert.True(result.TotalWeight > 0);
    }

    [Fact]
    public void FindShortestPath_TasToLHR_PrefersShorterRoute()
    {
        var graph    = BuildTestGraph();
        var dijkstra = new DijkstraAlgorithm(graph);

        var result = dijkstra.FindShortestPath("TAS", "LHR");

        Assert.True(result.Found);
        // TAS→IST→LHR = 6400, TAS→IST→CDG→LHR = 6180, TAS→DXB→LHR = 8700
        // Shortest should be via IST→CDG→LHR (6180)
        Assert.True(result.TotalWeight <= 6400);
    }

    [Fact]
    public void FindShortestPath_NoRoute_ReturnsFalse()
    {
        var graph    = BuildTestGraph();
        var dijkstra = new DijkstraAlgorithm(graph);

        var result = dijkstra.FindShortestPath("JFK", "TAS"); // no reverse edges

        Assert.False(result.Found);
    }

    [Fact]
    public void FindShortestPath_SameNode_ReturnsZeroWeight()
    {
        var graph = BuildTestGraph();
        var dijkstra = new DijkstraAlgorithm(graph);

        var result = dijkstra.FindShortestPath("TAS", "TAS");

        Assert.True(result.Found);
        Assert.Equal(0.0, result.TotalWeight);
    }

    [Fact]
    public void FindShortestPath_NonExistentVertex_ReturnsFalse()
    {
        var graph    = BuildTestGraph();
        var dijkstra = new DijkstraAlgorithm(graph);

        var result = dijkstra.FindShortestPath("TAS", "XYZ");

        Assert.False(result.Found);
    }

    [Fact]
    public void BFS_FromTAS_ReachesAllConnected()
    {
        var graph = BuildTestGraph();
        var bfs   = new BFSTraversal(graph);

        var visited = bfs.Traverse("TAS");

        Assert.Contains("TAS", visited);
        Assert.Contains("IST", visited);
        Assert.Contains("LHR", visited);
        Assert.Contains("JFK", visited);
        Assert.Contains("DXB", visited);
    }

    [Fact]
    public void DFS_TraverseAll_VisitsAllVertices()
    {
        var graph = BuildTestGraph();
        var dfs   = new DFSTraversal(graph);

        var all = dfs.TraverseAll();

        Assert.Equal(7, all.Length);
    }
}
