using Microsoft.AspNetCore.Mvc;
using SkyNet.Application.Services;

namespace SkyNet.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analytics;

    public AnalyticsController(AnalyticsService svc) => _analytics = svc;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try { return Ok(await _analytics.GetStatsAsync()); }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int count = 10)
    {
        try { return Ok(await _analytics.GetLeaderboardAsync(count)); }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    [HttpGet("benchmark")]
    public async Task<IActionResult> RunBenchmark()
    {
        try { return Ok(await _analytics.RunSortBenchmarkAsync()); }
        catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }
}
