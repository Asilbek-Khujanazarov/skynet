using Microsoft.EntityFrameworkCore;
using SkyNet.Domain.Entities;

namespace SkyNet.Infrastructure.Data;

public class SkyNetDbContext : DbContext
{
    public SkyNetDbContext(DbContextOptions<SkyNetDbContext> options) : base(options) { }

    public DbSet<Airport>   Airports   { get; set; } = null!;
    public DbSet<Flight>    Flights    { get; set; } = null!;
    public DbSet<Passenger> Passengers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airport>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.IataCode).IsUnique();
            e.Property(a => a.IataCode).HasMaxLength(4).IsRequired();
            e.Property(a => a.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Flight>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => f.FlightNumber).IsUnique();
            e.Property(f => f.FlightNumber).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<Passenger>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PNR).IsUnique();
            e.HasIndex(p => p.PassportId);
        });
    }
}
