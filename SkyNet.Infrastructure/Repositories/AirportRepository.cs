using Microsoft.EntityFrameworkCore;
using SkyNet.Domain.Entities;
using SkyNet.Infrastructure.Data;

namespace SkyNet.Infrastructure.Repositories;

public class AirportRepository
{
    private readonly IDbContextFactory<SkyNetDbContext> _factory;

    public AirportRepository(IDbContextFactory<SkyNetDbContext> factory) => _factory = factory;

    public async Task<List<Airport>> GetAllAsync()
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Airports.Where(a => a.IsActive).ToListAsync();
    }

    public async Task<List<Airport>> GetAllForGraphAsync()
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Airports.ToListAsync();
    }

    public async Task<Airport?> GetByIataAsync(string iata)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Airports.FirstOrDefaultAsync(a => a.IataCode == iata.ToUpper());
    }

    public async Task<List<Airport>> GetTopAsync(int count)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        return await ctx.Airports.OrderByDescending(a => a.UsageCount).Take(count).ToListAsync();
    }

    public async Task IncrementUsageAsync(string iata)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var a = await ctx.Airports.FirstOrDefaultAsync(a => a.IataCode == iata.ToUpper());
        if (a == null) return;
        a.UsageCount++;
        await ctx.SaveChangesAsync();
    }

    public async Task SetActiveAsync(string iata, bool active)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var a = await ctx.Airports.FirstOrDefaultAsync(a => a.IataCode == iata.ToUpper());
        if (a == null) return;
        a.IsActive = active;
        await ctx.SaveChangesAsync();
    }
}
