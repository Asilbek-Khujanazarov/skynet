using SkyNet.Domain.Enums;

namespace SkyNet.Domain.Entities;

public class Passenger
{
    public int Id { get; set; }
    public string PNR { get; set; } = string.Empty;         // e.g. "SKY-123456"
    public string PassportId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public TicketClass TicketClass { get; set; } = TicketClass.Economy;
    public string FlightNumber { get; set; } = string.Empty;
    public bool IsCheckedIn { get; set; } = false;
    public bool IsBoarded { get; set; } = false;
    public DateTime CheckInTime { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Priority score for MaxHeap: Platinum=3, Gold=2, Economy=1
    public int Priority => (int)TicketClass;
}
