namespace SkyNet.Domain.Entities;

public class Airport
{
    public int Id { get; set; }
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
}
