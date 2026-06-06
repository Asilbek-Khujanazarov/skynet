namespace SkyNet.Domain.Entities;

public class Route
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public double Distance { get; set; }   // km — used as weight in graph
    public double Cost { get; set; }       // USD
    public double DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}
