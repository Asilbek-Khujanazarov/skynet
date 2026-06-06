using SkyNet.Domain.Entities;
using SkyNet.Domain.Interfaces;
using SkyNet.DSA.Hashing;
using SkyNet.DSA.Sorting;
using SkyNet.DSA.Strings;
using SkyNet.DSA.Trees;
using SkyNet.Infrastructure.Repositories;

namespace SkyNet.Application.Services;

public class FlightService : IFlightService
{
    private readonly FlightRepository   _flightRepo;
    private readonly AirportRepository  _airportRepo;
    private readonly PassengerRepository _passengerRepo;

    // DSA structures
    private readonly AVLTree<double, Flight>   _priceTree   = new();
    private readonly BinarySearchTree<string, Flight> _flightBST = new();
    private readonly TrieTree                   _airportTrie = new();
    private readonly LRUCache<string, Flight>   _flightCache = new(200);
    private readonly KMPMatcher                 _kmp         = new();

    private bool _initialized = false;

    public FlightService(FlightRepository fr, AirportRepository ar, PassengerRepository pr)
    {
        _flightRepo    = fr;
        _airportRepo   = ar;
        _passengerRepo = pr;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        var flights  = await _flightRepo.GetAllAsync();
        var airports = await _airportRepo.GetAllAsync();

        // Index flights into AVLTree (by price) and BST (by flight number)
        foreach (var f in flights)
        {
            _priceTree.Insert(f.Price, f);
            _flightBST.Insert(f.FlightNumber, f);
        }

        // Index airport names into Trie
        foreach (var a in airports)
        {
            _airportTrie.Insert(a.Name,     a.IataCode);
            _airportTrie.Insert(a.IataCode, a.IataCode);
            _airportTrie.Insert(a.City,     a.IataCode);
        }

        _initialized = true;
    }

    public async Task<List<Flight>> GetScheduleAsync(DateTime date, string sortBy = "departure")
    {
        await EnsureInitializedAsync();
        var flights = await _flightRepo.GetByDateAsync(date);
        var arr     = flights.ToArray();

        if (sortBy == "fuel")
            MergeSort.Sort(arr, (a, b) => a.FuelEfficiency.CompareTo(b.FuelEfficiency));
        else
            QuickSort.Sort(arr, (a, b) => a.DepartureTime.CompareTo(b.DepartureTime));

        return arr.ToList();
    }

    public async Task<List<Flight>> SearchByPriceRangeAsync(double minPrice, double maxPrice)
    {
        await EnsureInitializedAsync();
        return _priceTree.RangeQuery(minPrice, maxPrice).ToList();
    }

    public async Task<List<Flight>> SearchByFlightNumberAsync(string query)
    {
        await EnsureInitializedAsync();
        var all = await _flightRepo.GetAllAsync();

        // Use KMP to search in flight numbers
        var names = all.Select(f => f.FlightNumber).ToArray();
        var idxs  = _kmp.SearchInList(names, query);

        return idxs.Select(i => all[i]).ToList();
    }

    public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
    {
        await EnsureInitializedAsync();
        var fn = flightNumber.ToUpper();

        // Try LRU cache first
        if (_flightCache.TryGet(fn, out var cached)) return cached;

        // Try BST O(log n)
        var flight = _flightBST.Search(fn);
        if (flight != null) { _flightCache.Put(fn, flight); return flight; }

        // Fall back to repository
        flight = await _flightRepo.GetByFlightNumberAsync(fn);
        if (flight != null) _flightCache.Put(fn, flight);
        return flight;
    }

    public async Task<List<Airport>> GetTopAirportsAsync(int count = 10)
        => await _airportRepo.GetTopAsync(count);

    public async Task<List<Airport>> AutoCompleteAirportAsync(string prefix)
    {
        await EnsureInitializedAsync();
        if (string.IsNullOrWhiteSpace(prefix)) return new List<Airport>();

        var results  = _airportTrie.AutoComplete(prefix.Trim(), 20);
        var iataCodes = results.Select(r => r.Metadata).Distinct().ToHashSet();

        var all = await _airportRepo.GetAllAsync();
        return all.Where(a => iataCodes.Contains(a.IataCode)).Take(10).ToList();
    }
}
