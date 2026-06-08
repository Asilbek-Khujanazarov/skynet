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

    // GET /api/flights/all?page=1&pageSize=50&search=
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        try
        {
            var all = await _flightService.GetAllFlightsAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToUpper();
                all = all.Where(f =>
                    f.FlightNumber.Contains(q) ||
                    f.OriginIata.Contains(q) ||
                    f.DestinationIata.Contains(q) ||
                    f.Status.ToString().ToUpper().Contains(q)
                ).ToList();
            }
            var total = all.Count;
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(f => new FlightDto
            {
                Id = f.Id, FlightNumber = f.FlightNumber,
                OriginIata = f.OriginIata, DestinationIata = f.DestinationIata,
                DepartureTime = f.DepartureTime, ArrivalTime = f.ArrivalTime,
                Price = f.Price, Distance = f.Distance,
                FuelEfficiency = f.FuelEfficiency, Status = f.Status.ToString(),
                SeatsAvailable = f.SeatsAvailable
            });
            return Ok(new { total, page, pageSize, items });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/flights
    [HttpPost]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.OriginIata) || string.IsNullOrWhiteSpace(req.DestinationIata))
                return BadRequest(new { error = "Qayerdan va qayerga majburiy." });

            var flight = await _flightService.CreateFlightAsync(req);

            await _hub.Clients.All.SendAsync("FlightStatusChanged", new
            {
                type = "FlightCreated",
                flightNumber = flight.FlightNumber,
                from = flight.OriginIata,
                to = flight.DestinationIata
            });

            return Ok(new FlightDto
            {
                Id = flight.Id, FlightNumber = flight.FlightNumber,
                OriginIata = flight.OriginIata, DestinationIata = flight.DestinationIata,
                DepartureTime = flight.DepartureTime, ArrivalTime = flight.ArrivalTime,
                Price = flight.Price, Distance = flight.Distance,
                FuelEfficiency = flight.FuelEfficiency, Status = flight.Status.ToString(),
                SeatsAvailable = flight.SeatsAvailable
            });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // DELETE /api/flights/{fn}
    [HttpDelete("{fn}")]
    public async Task<IActionResult> DeleteFlight(string fn)
    {
        try
        {
            await _flightService.DeleteFlightAsync(fn.ToUpper());
            return Ok(new { message = $"{fn} reysi o'chirildi." });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // PATCH /api/flights/{fn}/status
    [HttpPatch("{fn}/status")]
    public async Task<IActionResult> UpdateStatus(string fn, [FromBody] UpdateStatusRequest req)
    {
        try
        {
            await _flightService.UpdateStatusAsync(fn.ToUpper(), req.Status);
            await _hub.Clients.All.SendAsync("FlightStatusChanged", new { flightNumber = fn, status = req.Status });
            return Ok(new { message = "Holat yangilandi." });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
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

    // GET /api/flights/search?q=SK00  — KMP prefix search
    [HttpGet("search")]
    public async Task<IActionResult> SearchFlights([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(Array.Empty<object>());

            var results = await _flightService.SearchByFlightNumberAsync(q.ToUpper());
            return Ok(results.Take(10).Select(f => new FlightDto
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
