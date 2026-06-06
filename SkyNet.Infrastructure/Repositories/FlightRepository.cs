using Microsoft.EntityFrameworkCore;
using SkyNet.Domain.Entities;
using SkyNet.Domain.Enums;
using SkyNet.Infrastructure.Data;

namespace SkyNet.Infrastructure.Repositories;

public class FlightRepository
{
    private readonly IDbContextFactory<SkyNetDbContext> _factory;

    public FlightRepository(IDbContextFactory<SkyNetDbContext> factory) => _factory = factory;

    public async Task<List<Flight>> GetAllAsync()
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Flights.ToListAsync();
    }

    public async Task<List<Flight>> GetByDateAsync(DateTime date)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Flights
            .Where(f => f.DepartureTime.Date == date.Date)
            .ToListAsync();
    }

    public async Task<Flight?> GetByFlightNumberAsync(string fn)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Flights.FirstOrDefaultAsync(f => f.FlightNumber == fn.ToUpper());
    }

    public async Task<List<Flight>> GetByRouteAsync(string from, string to)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Flights
            .Where(f => f.OriginIata == from.ToUpper() && f.DestinationIata == to.ToUpper())
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(string flightNumber, FlightStatus status)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var f = await ctx.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber.ToUpper());
        if (f == null) return;
        f.Status = status;
        await ctx.SaveChangesAsync();
    }
}
