using DigitalDetox.Api.Data;
using DigitalDetox.Api.Dtos;
using DigitalDetox.Api.Models;
using DigitalDetox.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalDetox.Api.Controllers;

/// <summary>
/// Per-user settings (limits, challenges, streak). Requires authentication —
/// there is no anonymous access.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;
    public SettingsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<UserSettingsDto>> Get()
    {
        var userId = User.ResolveUserId();
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings == null)
        {
            settings = new UserSettings { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTime.UtcNow };
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return Ok(ToDto(settings));
    }

    [HttpPut]
    public async Task<ActionResult<UserSettingsDto>> Update([FromBody] UserSettingsDto dto)
    {
        var userId = User.ResolveUserId();
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings == null)
        {
            settings = new UserSettings { Id = Guid.NewGuid(), UserId = userId };
            _context.UserSettings.Add(settings);
        }

        settings.YouTubeLimit = Math.Max(0, dto.YouTubeLimit);
        settings.FacebookLimit = Math.Max(0, dto.FacebookLimit);
        settings.TikTokLimit = Math.Max(0, dto.TikTokLimit);
        settings.StreakCount = Math.Max(0, dto.StreakCount);
        settings.LastCompletedDay = dto.LastCompletedDay ?? string.Empty;
        settings.Challenges = (dto.Challenges ?? new List<ChallengeItemDto>())
            .Where(c => !string.IsNullOrWhiteSpace(c.Metric))
            .Select(c => new ChallengeItem
            {
                Id = string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString() : c.Id,
                Metric = c.Metric,
                Target = Math.Max(0, c.Target),
            })
            .ToList();
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ToDto(settings));
    }

    private static UserSettingsDto ToDto(UserSettings s) => new(
        s.YouTubeLimit,
        s.FacebookLimit,
        s.TikTokLimit,
        s.StreakCount,
        s.LastCompletedDay,
        s.Challenges.Select(c => new ChallengeItemDto(c.Id, c.Metric, c.Target)).ToList()
    );
}
