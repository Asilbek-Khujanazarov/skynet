using SkyNet.Application.DTOs;
using SkyNet.DSA.Graphs;
using SkyNet.DSA.Backtracking;
using SkyNet.Domain.Interfaces;
using SkyNet.Infrastructure.Repositories;

namespace SkyNet.Application.Services;

public class RouteService : IRouteService
{
    private readonly AirportRepository _airportRepo;
    private readonly FlightRepository _flightRepo;
    private AdjacencyListGraph? _graph;

    public RouteService(AirportRepository airportRepo, FlightRepository flightRepo)
    {
        _airportRepo = airportRepo;
        _flightRepo  = flightRepo;
    }

    // Build or return cached graph
    private async Task<AdjacencyListGraph> GetGraphAsync()
    {
        if (_graph != null) return _graph;

        var airports = await _airportRepo.GetAllForGraphAsync();
        var flights  = await _flightRepo.GetAllAsync();

        _graph = new AdjacencyListGraph(airports.Count + 50, 500);

        foreach (var a in airports)
            _graph.AddVertex(a.IataCode);

        foreach (var f in flights)
            _graph.AddUndirectedEdge(f.OriginIata, f.DestinationIata, f.Distance);

        return _graph;
    }

    public async Task<Domain.Interfaces.RouteResult?> FindShortestPathAsync(string from, string to)
    {
        var graph   = await GetGraphAsync();
        var dijkstra = new DijkstraAlgorithm(graph);
        var result  = dijkstra.FindShortestPath(from.ToUpper(), to.ToUpper());

        if (!result.Found) return null;

        // Increment usage counters
        await _airportRepo.IncrementUsageAsync(from);
        await _airportRepo.IncrementUsageAsync(to);

        return new Domain.Interfaces.RouteResult
        {
            Path          = result.Path.ToList(),
            TotalDistance = result.TotalWeight,
            TotalCost     = result.TotalWeight * 0.06  // $0.06 per km approximation
        };
    }

    public async Task<List<Domain.Interfaces.RouteEdge>> GetMSTAsync()
    {
        var graph  = await GetGraphAsync();
        var kruskal = new KruskalMST();
        var mst    = kruskal.Compute(graph);

        return mst.Edges.Select(e => new Domain.Interfaces.RouteEdge
        {
            From   = e.From,
            To     = e.To,
            Weight = e.Weight
        }).ToList();
    }

    public async Task<List<List<string>>> FindAllAlternativeRoutesAsync(string from, string to, string closed)
    {
        var airports = await _airportRepo.GetAllForGraphAsync();
        var flights  = await _flightRepo.GetAllAsync();
        int n = airports.Count;

        var vertices = airports.Select(a => a.IataCode).ToArray();

        // Build O(1) index lookup
        var indexMap = new Dictionary<string, int>(n);
        for (int i = 0; i < n; i++) indexMap[vertices[i]] = i;

        var tempAdj = new List<string>[n];
        var tempWt  = new List<double>[n];
        for (int i = 0; i < n; i++) { tempAdj[i] = new(); tempWt[i] = new(); }

        foreach (var f in flights)
        {
            if (indexMap.TryGetValue(f.OriginIata, out int fi) &&
                indexMap.TryGetValue(f.DestinationIata, out int ti))
            {
                tempAdj[fi].Add(f.DestinationIata);
                tempWt[fi].Add(f.Distance);
                // undirected
                tempAdj[ti].Add(f.OriginIata);
                tempWt[ti].Add(f.Distance);
            }
        }

        var adj     = new string[n][];
        var weights = new double[n][];
        for (int i = 0; i < n; i++)
        {
            adj[i]     = tempAdj[i].ToArray();
            weights[i] = tempWt[i].ToArray();
        }

        var backtracker = new RouteBacktracker(vertices, adj, weights, indexMap, maxDepth: 4);
        var result      = backtracker.FindAlternativeRoutes(from.ToUpper(), to.ToUpper(), closed.ToUpper());

        return result.Paths.Select(p => p.ToList()).ToList();
    }

    public async Task<List<string>> GetConnectedAirportsBFSAsync(string startIata)
    {
        var graph = await GetGraphAsync();
        var bfs   = new BFSTraversal(graph);
        return bfs.Traverse(startIata.ToUpper()).ToList();
    }

    public async Task<List<string>> GetAllAirportsDFSAsync(string startIata)
    {
        var graph = await GetGraphAsync();
        var dfs   = new DFSTraversal(graph);
        return dfs.Traverse(startIata.ToUpper()).ToList();
    }

    public void InvalidateGraph() => _graph = null;
}
