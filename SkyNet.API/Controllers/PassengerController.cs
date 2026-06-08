using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SkyNet.API.Hubs;
using SkyNet.Application.DTOs;
using SkyNet.Application.Services;
using SkyNet.Domain.Entities;
using SkyNet.Domain.Enums;

namespace SkyNet.API.Controllers;

[ApiController]
[Route("api")]
public class PassengerController : ControllerBase
{
    private readonly PassengerService _service;
    private readonly IHubContext<SkyNetHub> _hub;

    public PassengerController(PassengerService svc, IHubContext<SkyNetHub> hub)
    {
        _service = svc;
        _hub     = hub;
    }

    // GET /api/passengers/{pnr}
    [HttpGet("passengers/{pnr}")]
    public async Task<IActionResult> GetByPNR(string pnr)
    {
        try
        {
            var p = await _service.LookupByPNRAsync(pnr);
            if (p == null) return NotFound(new { error = $"PNR '{pnr}' bo'yicha yo'lovchi topilmadi." });
            return Ok(ToDto(p));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/passengers/passport/{id}
    [HttpGet("passengers/passport/{id}")]
    public async Task<IActionResult> GetByPassport(string id)
    {
        try
        {
            var p = await _service.LookupByPassportAsync(id);
            if (p == null) return NotFound(new { error = $"Pasport '{id}' bo'yicha yo'lovchi topilmadi." });
            return Ok(ToDto(p));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/checkin
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest req)
    {
        try
        {
            if (!Enum.TryParse<TicketClass>(req.TicketClass, true, out var cls))
                cls = TicketClass.Economy;

            var passenger = new Passenger
            {
                PNR          = string.IsNullOrEmpty(req.PNR) ? $"SKY-{Guid.NewGuid().ToString()[..6].ToUpper()}" : req.PNR,
                PassportId   = req.PassportId,
                FirstName    = req.FirstName,
                LastName     = req.LastName,
                Nationality  = req.Nationality,
                TicketClass  = cls,
                FlightNumber = req.FlightNumber.ToUpper(),
            };

            await _service.CheckInAsync(passenger);

            // Notify via SignalR
            var queue = await _service.GetQueueAsync(req.FlightNumber);
            await _hub.Clients.All.SendAsync("QueueUpdated", new
            {
                flight    = req.FlightNumber,
                queueSize = queue.Count
            });

            return Ok(new { message = "Ro'yxatdan o'tish muvaffaqiyatli.", pnr = passenger.PNR, priority = passenger.Priority });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/boarding/queue?flight=SK0010
    [HttpGet("boarding/queue")]
    public async Task<IActionResult> GetBoardingQueue([FromQuery] string flight)
    {
        try
        {
            var queue = await _service.GetQueueAsync(flight);
            return Ok(queue.Select(ToDto));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/boarding/next?flight=SK0010
    [HttpPost("boarding/next")]
    public async Task<IActionResult> BoardNext([FromQuery] string flight)
    {
        try
        {
            var passenger = await _service.BoardNextAsync(flight);
            if (passenger == null) return Ok(new { message = "Navbatda yo'lovchi qolmadi." });

            await _hub.Clients.All.SendAsync("PassengerBoarded", new
            {
                name   = passenger.FullName,
                cls    = passenger.TicketClass.ToString(),
                flight = flight
            });

            return Ok(ToDto(passenger));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/boarding/gate?flight=SK0010
    [HttpGet("boarding/gate")]
    public async Task<IActionResult> GetBoardingGate([FromQuery] string flight)
    {
        try
        {
            var gate = await _service.GetBoardingGateAsync(flight);
            return Ok(gate.Select(ToDto));
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/cargo/load
    [HttpPost("cargo/load")]
    public async Task<IActionResult> LoadCargo([FromBody] CargoRequest req)
    {
        try
        {
            await _service.LoadCargoAsync(req.Item);
            var stack = await _service.GetCargoStackAsync();
            return Ok(new { message = $"'{req.Item}' yuklandi.", stackSize = stack.Count });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // POST /api/cargo/unload
    [HttpPost("cargo/unload")]
    public async Task<IActionResult> UnloadCargo()
    {
        try
        {
            var item = await _service.UnloadCargoAsync();
            if (item == null) return Ok(new { message = "Yuk bo'limi bo'sh." });
            return Ok(new { unloaded = item });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    // GET /api/cargo/stack
    [HttpGet("cargo/stack")]
    public async Task<IActionResult> GetCargoStack()
    {
        try
        {
            var stack = await _service.GetCargoStackAsync();
            return Ok(new { items = stack, count = stack.Count });
        }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    private static PassengerDto ToDto(Passenger p) => new()
    {
        PNR          = p.PNR,
        PassportId   = p.PassportId,
        FullName     = p.FullName,
        Nationality  = p.Nationality,
        TicketClass  = p.TicketClass.ToString(),
        FlightNumber = p.FlightNumber,
        IsCheckedIn  = p.IsCheckedIn,
        IsBoarded    = p.IsBoarded,
        Priority     = p.Priority
    };
}

public class CargoRequest
{
    public string Item { get; set; } = string.Empty;
}
