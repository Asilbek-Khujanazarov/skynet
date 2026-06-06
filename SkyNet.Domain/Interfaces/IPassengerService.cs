using SkyNet.Domain.Entities;

namespace SkyNet.Domain.Interfaces;

public interface IPassengerService
{
    Task<bool> CheckInAsync(Passenger passenger);
    Task<Passenger?> BoardNextAsync(string flightNumber);
    Task<List<Passenger>> GetQueueAsync(string flightNumber);
    Task<Passenger?> LookupByPNRAsync(string pnr);
    Task<Passenger?> LookupByPassportAsync(string passportId);
    Task<bool> LoadCargoAsync(string item);
    Task<string?> UnloadCargoAsync();
    Task<List<string>> GetCargoStackAsync();
}
