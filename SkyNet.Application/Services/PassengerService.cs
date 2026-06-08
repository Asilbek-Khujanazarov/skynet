using SkyNet.Domain.Entities;
using SkyNet.Domain.Enums;
using SkyNet.Domain.Interfaces;
using SkyNet.DSA.Hashing;
using SkyNet.DSA.Queues;
using SkyNet.Infrastructure.Repositories;

namespace SkyNet.Application.Services;

public class PassengerService : IPassengerService
{
    private readonly PassengerRepository _repo;

    // DSA structures — in-memory, per application lifecycle
    private readonly HashTable<string, Passenger> _pnrIndex     = new(512);
    private readonly HashTable<string, Passenger> _passportIndex = new(512);

    // Per-flight queues — key: flightNumber
    private readonly HashTable<string, PriorityQueue<Passenger>>  _checkInQueues = new(64);
    private readonly HashTable<string, CircularQueue<Passenger>>  _boardingQueues = new(64);
    private readonly LinkedStack<string>                           _cargoStack     = new();

    private bool _indexed = false;

    public PassengerService(PassengerRepository repo) => _repo = repo;

    private async Task EnsureIndexedAsync()
    {
        if (_indexed) return;
        var all = await _repo.GetAllAsync();
        foreach (var p in all)
        {
            _pnrIndex.Set(p.PNR, p);
            _passportIndex.Set(p.PassportId, p);

            // Restore into priority queues so data survives page navigation
            if (p.IsCheckedIn && !p.IsBoarded)
            {
                if (!_checkInQueues.ContainsKey(p.FlightNumber))
                    _checkInQueues.Set(p.FlightNumber, new PriorityQueue<Passenger>(256));
                _checkInQueues.Get(p.FlightNumber)!.Enqueue(p, p.Priority);
            }
        }
        _indexed = true;
    }

    public async Task<bool> CheckInAsync(Passenger passenger)
    {
        await EnsureIndexedAsync();

        passenger.IsCheckedIn  = true;
        passenger.CheckInTime  = DateTime.UtcNow;

        // Add/update DB
        var existing = await _repo.GetByPNRAsync(passenger.PNR);
        if (existing != null) { existing.IsCheckedIn = true; await _repo.UpdateAsync(existing); }
        else                  { await _repo.AddAsync(passenger); }

        // Update DSA indices
        _pnrIndex.Set(passenger.PNR, passenger);
        _passportIndex.Set(passenger.PassportId, passenger);

        // Add to priority queue for this flight
        if (!_checkInQueues.ContainsKey(passenger.FlightNumber))
            _checkInQueues.Set(passenger.FlightNumber, new PriorityQueue<Passenger>(256));

        _checkInQueues.Get(passenger.FlightNumber)!.Enqueue(passenger, passenger.Priority);

        return true;
    }

    public async Task<Passenger?> BoardNextAsync(string flightNumber)
    {
        await EnsureIndexedAsync();

        var checkinQ = _checkInQueues.Get(flightNumber);
        if (checkinQ == null || checkinQ.IsEmpty) return null;

        var passenger = checkinQ.Dequeue();
        passenger.IsBoarded = true;

        // Add to FIFO boarding gate
        if (!_boardingQueues.ContainsKey(flightNumber))
            _boardingQueues.Set(flightNumber, new CircularQueue<Passenger>(300));
        _boardingQueues.Get(flightNumber)!.Enqueue(passenger);

        var db = await _repo.GetByPNRAsync(passenger.PNR);
        if (db != null) { db.IsBoarded = true; await _repo.UpdateAsync(db); }

        return passenger;
    }

    public async Task<List<Passenger>> GetQueueAsync(string flightNumber)
    {
        await EnsureIndexedAsync();
        var queue = _checkInQueues.Get(flightNumber);
        if (queue == null) return new List<Passenger>();
        // Sort by priority desc (Platinum=3 first), then by check-in time asc (FIFO within same class)
        return queue.ToArray()
                    .OrderByDescending(p => p.Priority)
                    .ThenBy(p => p.CheckInTime)
                    .ToList();
    }

    public async Task<List<Passenger>> GetBoardingGateAsync(string flightNumber)
    {
        await EnsureIndexedAsync();
        var gate = _boardingQueues.Get(flightNumber);
        if (gate == null) return new List<Passenger>();
        return gate.ToArray().ToList();
    }

    public async Task<Passenger?> LookupByPNRAsync(string pnr)
    {
        await EnsureIndexedAsync();
        var cached = _pnrIndex.Get(pnr);
        if (cached != null) return cached;
        return await _repo.GetByPNRAsync(pnr);
    }

    public async Task<Passenger?> LookupByPassportAsync(string passportId)
    {
        await EnsureIndexedAsync();
        var cached = _passportIndex.Get(passportId);
        if (cached != null) return cached;
        return await _repo.GetByPassportAsync(passportId);
    }

    public Task<bool> LoadCargoAsync(string item)
    {
        _cargoStack.Push(item);
        return Task.FromResult(true);
    }

    public Task<string?> UnloadCargoAsync()
    {
        if (_cargoStack.IsEmpty) return Task.FromResult<string?>(null);
        return Task.FromResult<string?>(_cargoStack.Pop());
    }

    public Task<List<string>> GetCargoStackAsync()
        => Task.FromResult(_cargoStack.ToArray().ToList());
}
