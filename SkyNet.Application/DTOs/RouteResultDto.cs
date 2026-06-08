namespace SkyNet.Application.DTOs;

public class RouteResultDto
{
    public bool Found { get; set; }
    public string[] Path { get; set; } = Array.Empty<string>();
    public double TotalDistance { get; set; }
    public double TotalCost { get; set; }
    public int Stops { get; set; }
    public string Algorithm { get; set; } = "Dijkstra";
}

public class MSTEdgeDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public double Weight { get; set; }
}

public class MSTResultDto
{
    public MSTEdgeDto[] Edges { get; set; } = Array.Empty<MSTEdgeDto>();
    public double TotalWeight { get; set; }
    public int EdgeCount { get; set; }
}

public class CreateFlightRequest
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public double Price { get; set; }
    public double Distance { get; set; }
    public int SeatsTotal { get; set; } = 180;
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class RerouteRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string ClosedAirport { get; set; } = string.Empty;
}

public class RerouteResultDto
{
    public int TotalAlternatives { get; set; }
    public AlternativeRouteDto[] Routes { get; set; } = Array.Empty<AlternativeRouteDto>();
}

public class AlternativeRouteDto
{
    public string[] Path { get; set; } = Array.Empty<string>();
    public double TotalCost { get; set; }
    public int Hops { get; set; }
}
