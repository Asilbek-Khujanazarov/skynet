using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SkyNet.API.Hubs;
using SkyNet.Application.DTOs;
using SkyNet.Application.Services;

namespace SkyNet.API.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightController : ControllerBase
{
    private readonly RouteService  _routeService;
    private readonly FlightService _flightService;
    private readonly IHubContext<SkyNetHub> _hub;

    public FlightController(RouteService rs, FlightService fs, IHubContext<SkyNetHub> hub)
    {
        _routeService  = rs;
        _flightService = fs;
        _hub           = hub;
    }

    // GET /api/flights/route?from=TAS&to=LHR
    [HttpGet("route")]
    public async Task<IActionResult> GetRoute([FromQuery] string from, [FromQuery] string to)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return BadRequest(new { error = "Qayerdan va qayerga borishi majburiy." });

            var result = await _routeService.FindShortestPathAsync(from, to);
            if (result == null)
                return NotFound(new { error = $"{from} dan {to} ga marshrut topilmadi." });

            return Ok(new RouteResultDto
            {
                Found         = true,
                Path          = result.Path.ToArray(),
                TotalDistance = result.TotalDistance,
                TotalCost     = result.TotalCost,
                Stops         = result.Stops,
                Algorithm     = "Dijkstra O((V+E) log V)"
            });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/schedule?date=2026-06-05&sortBy=departure
    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule([FromQuery] DateTime date, [FromQuery] string sortBy = "departure")
    {
        try
        {
            var flights = await _flightService.GetScheduleAsync(date, sortBy);
            return Ok(flights.Select(f => new FlightDto
            {
                Id              = f.Id,
                FlightNumber    = f.FlightNumber,
                OriginIata      = f.OriginIata,
                DestinationIata = f.DestinationIata,
                DepartureTime   = f.DepartureTime,
                ArrivalTime     = f.ArrivalTime,
                Price           = f.Price,
                Distance        = f.Distance,
                FuelEfficiency  = f.FuelEfficiency,
                Status          = f.Status.ToString(),
                SeatsAvailable  = f.SeatsAvailable
            }));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/mst
    [HttpGet("mst")]
    public async Task<IActionResult> GetMST()
    {
        try
        {
            var edges = await _routeService.GetMSTAsync();
            return Ok(new MSTResultDto
            {
                Edges       = edges.Select(e => new MSTEdgeDto { From = e.From, To = e.To, Weight = e.Weight }).ToArray(),
                TotalWeight = edges.Sum(e => e.Weight),
                EdgeCount   = edges.Count
            });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/flights/reroute
    [HttpPost("reroute")]
    public async Task<IActionResult> Reroute([FromBody] RerouteRequest request)
    {
        try
        {
            var paths = await _routeService.FindAllAlternativeRoutesAsync(
                request.From, request.To, request.ClosedAirport);

            // Notify all clients via SignalR
            await _hub.Clients.All.SendAsync("EmergencyAlert", new
            {
                type    = "AirportClosed",
                airport = request.ClosedAirport,
                routes  = paths.Count
            });

            return Ok(new RerouteResultDto
            {
                TotalAlternatives = paths.Count,
                Routes = paths.Select(p => new AlternativeRouteDto
                {
                    Path  = p.ToArray(),
                    Hops  = p.Count - 1
                }).ToArray()
            });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/bfs?start=TAS
    [HttpGet("bfs")]
    public async Task<IActionResult> BFS([FromQuery] string start)
    {
        try
        {
            var result = await _routeService.GetConnectedAirportsBFSAsync(start);
            return Ok(new { start, algorithm = "BFS O(V+E)", airports = result });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/dfs?start=TAS
    [HttpGet("dfs")]
    public async Task<IActionResult> DFS([FromQuery] string start)
    {
        try
        {
            var result = await _routeService.GetAllAirportsDFSAsync(start);
            return Ok(new { start, algorithm = "DFS O(V+E)", airports = result });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/number/{fn}
    [HttpGet("number/{fn}")]
    public async Task<IActionResult> GetFlight(string fn)
    {
        try
        {
            var flight = await _flightService.GetByFlightNumberAsync(fn);
            if (flight == null) return NotFound(new { error = $"{fn} reysi topilmadi." });
            return Ok(new FlightDto
            {
                Id              = flight.Id,
                FlightNumber    = flight.FlightNumber,
                OriginIata      = flight.OriginIata,
                DestinationIata = flight.DestinationIata,
                DepartureTime   = flight.DepartureTime,
                ArrivalTime     = flight.ArrivalTime,
                Price           = flight.Price,
                Distance        = flight.Distance,
                FuelEfficiency  = flight.FuelEfficiency,
                Status          = flight.Status.ToString(),
                SeatsAvailable  = flight.SeatsAvailable
            });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/flights/price?min=100&max=500
    [HttpGet("price")]
    public async Task<IActionResult> SearchByPrice([FromQuery] double min, [FromQuery] double max)
    {
        try
        {
            var results = await _flightService.SearchByPriceRangeAsync(min, max);
            return Ok(results.Select(f => new FlightDto
            {
                Id              = f.Id,
                FlightNumber    = f.FlightNumber,
                OriginIata      = f.OriginIata,
                DestinationIata = f.DestinationIata,
                DepartureTime   = f.DepartureTime,
                ArrivalTime     = f.ArrivalTime,
                Price           = f.Price,
                Distance        = f.Distance,
                FuelEfficiency  = f.FuelEfficiency,
                Status          = f.Status.ToString(),
                SeatsAvailable  = f.SeatsAvailable
            }));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }
}
