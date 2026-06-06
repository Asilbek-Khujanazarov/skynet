using SkyNet.Domain.Enums;

namespace SkyNet.Domain.Entities;

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public double Price { get; set; }
    public double Distance { get; set; }        // km
    public double FuelEfficiency { get; set; }  // L per 100km
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public int SeatsTotal { get; set; }
    public int SeatsAvailable { get; set; }
}
