namespace SkyNet.Application.DTOs;

public class FlightDto
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public double Price { get; set; }
    public double Distance { get; set; }
    public double FuelEfficiency { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SeatsAvailable { get; set; }
}
