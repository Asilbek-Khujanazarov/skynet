using Microsoft.EntityFrameworkCore;
using SkyNet.Domain.Entities;
using SkyNet.Infrastructure.Data;

namespace SkyNet.Infrastructure.Repositories;

public class PassengerRepository
{
    private readonly IDbContextFactory<SkyNetDbContext> _factory;

    public PassengerRepository(IDbContextFactory<SkyNetDbContext> factory) => _factory = factory;

    public async Task<List<Passenger>> GetAllAsync()
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Passengers.ToListAsync();
    }

    public async Task<Passenger?> GetByPNRAsync(string pnr)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Passengers.FirstOrDefaultAsync(p => p.PNR == pnr);
    }

    public async Task<Passenger?> GetByPassportAsync(string passportId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Passengers.FirstOrDefaultAsync(p => p.PassportId == passportId);
    }

    public async Task<List<Passenger>> GetByFlightAsync(string flightNumber)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Passengers.Where(p => p.FlightNumber == flightNumber).ToListAsync();
    }

    public async Task AddAsync(Passenger p)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        await ctx.Passengers.AddAsync(p);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Passenger p)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        ctx.Passengers.Update(p);
        await ctx.SaveChangesAsync();
    }
}
