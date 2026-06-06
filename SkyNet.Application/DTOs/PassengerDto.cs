namespace SkyNet.Application.DTOs;

public class PassengerDto
{
    public string PNR { get; set; } = string.Empty;
    public string PassportId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string TicketClass { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public bool IsCheckedIn { get; set; }
    public bool IsBoarded { get; set; }
    public int Priority { get; set; }
}

public class CheckInRequest
{
    public string PNR { get; set; } = string.Empty;
    public string PassportId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string TicketClass { get; set; } = "Economy";
    public string Nationality { get; set; } = string.Empty;
}
