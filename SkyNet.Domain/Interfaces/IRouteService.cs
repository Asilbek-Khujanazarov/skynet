using SkyNet.Domain.Entities;

namespace SkyNet.Domain.Interfaces;

public interface IRouteService
{
    Task<RouteResult?> FindShortestPathAsync(string fromIata, string toIata);
    Task<List<RouteEdge>> GetMSTAsync();
    Task<List<List<string>>> FindAllAlternativeRoutesAsync(string fromIata, string toIata, string closedAirport);
    Task<List<string>> GetConnectedAirportsBFSAsync(string startIata);
    Task<List<string>> GetAllAirportsDFSAsync(string startIata);
}

public class RouteResult
{
    public List<string> Path { get; set; } = new();
    public double TotalDistance { get; set; }
    public double TotalCost { get; set; }
    public int Stops => Path.Count - 2;
}

public class RouteEdge
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public double Weight { get; set; }
}
