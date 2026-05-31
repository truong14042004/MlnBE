using DigitalDetox.Api.Data;
using DigitalDetox.Api.Dtos;
using DigitalDetox.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalDetox.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;
    public StatsController(AppDbContext context) => _context = context;

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> Summary([FromQuery] int days = 7)
    {
        var userId = User.ResolveUserId();
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var logs = await _context.ScreenTimeLogs
            .Where(l => l.UserId == userId && l.CreatedAt >= cutoff)
            .ToListAsync();

        var byWebsite = logs
            .GroupBy(l => l.Website)
            .Select(g => new SiteTotalDto(g.Key, g.Sum(x => x.DurationSeconds)))
            .OrderByDescending(s => s.TotalSeconds)
            .ToList();

        // Build rolling daily map (oldest -> newest) so the chart x-axis is stable.
        var today = DateTime.UtcNow.Date;
        var daily = new List<DailyTotalDto>();
        for (int i = days - 1; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var key = d.ToString("yyyy-MM-dd");
            var dayLogs = logs.Where(l => l.Day == key).ToList();
            var total = dayLogs.Sum(l => l.DurationSeconds);
            var map = dayLogs
                .GroupBy(l => l.Website)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.DurationSeconds));
            daily.Add(new DailyTotalDto(key, total, map));
        }

        var totalSeconds = logs.Sum(l => l.DurationSeconds);
        var score = RecommendationEngine.AwarenessScore(totalSeconds);
        var tips = RecommendationEngine.Generate(logs);

        return Ok(new SummaryDto(totalSeconds, score, byWebsite, daily, tips));
    }
}
