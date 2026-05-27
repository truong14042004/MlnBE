using DigitalDetox.Api.Data;
using DigitalDetox.Api.Dtos;
using DigitalDetox.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalDetox.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrackingController(AppDbContext context) => _context = context;

    /// <summary>Single log (kept for backward compatibility with the spec).</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] ScreenTimeLog model)
    {
        if (string.IsNullOrWhiteSpace(model.Website)) return BadRequest("Website required");
        if (string.IsNullOrWhiteSpace(model.Day))
            model.Day = DateTime.UtcNow.ToString("yyyy-MM-dd");
        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;
        _context.ScreenTimeLogs.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    /// <summary>Batched upload from extension (preferred).</summary>
    [HttpPost("batch")]
    public async Task<IActionResult> SaveBatch([FromBody] TrackingBatchDto batch)
    {
        if (batch.Entries == null || batch.Entries.Count == 0) return Ok(new { saved = 0 });
        var now = DateTime.UtcNow;
        var rows = batch.Entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Website) && e.DurationSeconds > 0)
            .Select(e => new ScreenTimeLog
            {
                Id = Guid.NewGuid(),
                UserId = string.IsNullOrWhiteSpace(batch.UserId) ? "anon" : batch.UserId,
                Website = e.Website,
                DurationSeconds = e.DurationSeconds,
                Day = string.IsNullOrWhiteSpace(batch.Day) ? now.ToString("yyyy-MM-dd") : batch.Day,
                CreatedAt = now,
            })
            .ToList();

        _context.ScreenTimeLogs.AddRange(rows);
        await _context.SaveChangesAsync();
        return Ok(new { saved = rows.Count });
    }

    /// <summary>Raw logs for advanced views.</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string userId = "anon", [FromQuery] int days = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var logs = await _context.ScreenTimeLogs
            .Where(l => l.UserId == userId && l.CreatedAt >= cutoff)
            .OrderByDescending(l => l.CreatedAt)
            .Take(2000)
            .ToListAsync();
        return Ok(logs);
    }
}
