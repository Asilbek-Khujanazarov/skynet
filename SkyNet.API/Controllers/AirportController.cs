using Microsoft.AspNetCore.Mvc;
using SkyNet.Application.Services;
using SkyNet.Infrastructure.Repositories;

namespace SkyNet.API.Controllers;

[ApiController]
[Route("api/airports")]
public class AirportController : ControllerBase
{
    private readonly AirportRepository _repo;
    private readonly FlightService     _flightService;

    public AirportController(AirportRepository repo, FlightService fs)
    {
        _repo          = repo;
        _flightService = fs;
    }

    // GET /api/airports
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var airports = await _repo.GetAllAsync();
            return Ok(airports);
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/airports/top10
    [HttpGet("top10")]
    public async Task<IActionResult> GetTop10()
    {
        try
        {
            var top = await _flightService.GetTopAirportsAsync(10);
            return Ok(top);
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/airports/search?q=london
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "'q' parametri majburiy." });

            var results = await _flightService.AutoCompleteAirportAsync(q);
            return Ok(results);
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/airports/iata/{code}
    [HttpGet("iata/{iata}")]
    public async Task<IActionResult> GetByIata(string iata)
    {
        try
        {
            var airport = await _repo.GetByIataAsync(iata);
            if (airport == null) return NotFound(new { error = $"'{iata}' aeroporti topilmadi." });
            return Ok(airport);
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // PATCH /api/airports/iata/{code}/close
    [HttpPatch("iata/{iata}/close")]
    public async Task<IActionResult> CloseAirport(string iata)
    {
        try
        {
            await _repo.SetActiveAsync(iata, false);
            return Ok(new { message = $"{iata} aeroporti yopildi." });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // PATCH /api/airports/iata/{code}/open
    [HttpPatch("iata/{iata}/open")]
    public async Task<IActionResult> OpenAirport(string iata)
    {
        try
        {
            await _repo.SetActiveAsync(iata, true);
            return Ok(new { message = $"{iata} aeroporti qayta ochildi." });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }
}
