using Microsoft.EntityFrameworkCore;
using SkyNet.API.Hubs;
using SkyNet.Application.Services;
using SkyNet.Infrastructure.Data;
using SkyNet.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// EF Core InMemory — factory pattern (thread-safe, Singleton-friendly)
builder.Services.AddDbContextFactory<SkyNetDbContext>(opt =>
    opt.UseInMemoryDatabase("SkyNetDB"));

// Repositories — Singleton because they hold IDbContextFactory (thread-safe)
builder.Services.AddSingleton<AirportRepository>();
builder.Services.AddSingleton<FlightRepository>();
builder.Services.AddSingleton<PassengerRepository>();

// Application Services — Singleton so DSA in-memory state persists
builder.Services.AddSingleton<RouteService>();
builder.Services.AddSingleton<PassengerService>();
builder.Services.AddSingleton<FlightService>();
builder.Services.AddSingleton<AnalyticsService>();

// CORS — allow frontend; SignalR requires credentials (no wildcard origin)
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(_ => true)
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseDefaultFiles();   // serves index.html from wwwroot
app.UseStaticFiles();    // serves wwwroot

app.MapControllers();
app.MapHub<SkyNetHub>("/hubs/skynet");

// ── Seed data on startup ──────────────────────────────────────────
{
    var factory = app.Services.GetRequiredService<IDbContextFactory<SkyNetDbContext>>();
    await using var ctx = await factory.CreateDbContextAsync();
    var wwwroot = app.Environment.WebRootPath ?? "wwwroot";
    ctx.Database.EnsureCreated();
    await DataSeeder.SeedAsync(ctx, wwwroot);
}

app.Run();
