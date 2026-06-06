using SkyNet.Domain.Entities;

namespace SkyNet.Domain.Interfaces;

public interface IFlightService
{
    Task<List<Flight>> GetScheduleAsync(DateTime date, string sortBy = "departure");
    Task<List<Flight>> SearchByPriceRangeAsync(double minPrice, double maxPrice);
    Task<List<Flight>> SearchByFlightNumberAsync(string query);
    Task<Flight?> GetByFlightNumberAsync(string flightNumber);
    Task<List<Airport>> GetTopAirportsAsync(int count = 10);
    Task<List<Airport>> AutoCompleteAirportAsync(string prefix);
}
