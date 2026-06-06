using SkyNet.Domain.Entities;
using SkyNet.DSA.Sorting;
using SkyNet.Infrastructure.Repositories;

namespace SkyNet.Application.Services;

public class AnalyticsService
{
    private readonly AirportRepository  _airportRepo;
    private readonly FlightRepository   _flightRepo;
    private readonly PassengerRepository _passengerRepo;

    public AnalyticsService(AirportRepository ar, FlightRepository fr, PassengerRepository pr)
    {
        _airportRepo   = ar;
        _flightRepo    = fr;
        _passengerRepo = pr;
    }

    public async Task<GlobalStats> GetStatsAsync()
    {
        var airports   = await _airportRepo.GetAllAsync();
        var flights    = await _flightRepo.GetAllAsync();
        var passengers = await _passengerRepo.GetAllAsync();

        return new GlobalStats
        {
            TotalAirports    = airports.Count,
            TotalFlights     = flights.Count,
            TotalPassengers  = passengers.Count,
            CheckedIn        = passengers.Count(p => p.IsCheckedIn),
            Boarded          = passengers.Count(p => p.IsBoarded),
            AvgFlightPrice   = flights.Count > 0 ? flights.Average(f => f.Price) : 0,
            ActiveFlights    = flights.Count(f => f.Status == Domain.Enums.FlightStatus.InFlight),
        };
    }

    public async Task<List<Airport>> GetLeaderboardAsync(int count = 10)
    {
        var all = (await _airportRepo.GetAllAsync()).ToArray();
        QuickSort.Sort(all, (a, b) => b.UsageCount.CompareTo(a.UsageCount));
        return all.Take(count).ToList();
    }

    public async Task<SortBenchmarkStats> RunSortBenchmarkAsync()
    {
        var flights = (await _flightRepo.GetAllAsync()).ToArray();
        var bench   = new SortBenchmark();
        var result  = bench.Run(flights, (a, b) => a.DepartureTime.CompareTo(b.DepartureTime));

        return new SortBenchmarkStats
        {
            DataSize        = result.DataSize,
            QuickSortMs     = result.QuickSortMs,
            MergeSortMs     = result.MergeSortMs,
            FasterAlgorithm = result.FasterAlgorithm
        };
    }
}

public class GlobalStats
{
    public int TotalAirports   { get; set; }
    public int TotalFlights    { get; set; }
    public int TotalPassengers { get; set; }
    public int CheckedIn       { get; set; }
    public int Boarded         { get; set; }
    public double AvgFlightPrice { get; set; }
    public int ActiveFlights   { get; set; }
}

public class SortBenchmarkStats
{
    public int DataSize         { get; set; }
    public long QuickSortMs     { get; set; }
    public long MergeSortMs     { get; set; }
    public string FasterAlgorithm { get; set; } = string.Empty;
}
