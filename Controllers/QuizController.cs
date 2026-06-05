using DigitalDetox.Api.Data;
using DigitalDetox.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalDetox.Api.Controllers;

/// <summary>
/// Serves quiz questions for the extension's practice overlay. Anonymous access
/// is allowed because the block overlay can appear before the extension has an
/// authenticated session.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class QuizController : ControllerBase
{
    private readonly AppDbContext _context;
    public QuizController(AppDbContext context) => _context = context;

    /// <summary>
    /// Returns a random batch of questions (default 15) for one practice session.
    /// </summary>
    [HttpGet("questions")]
    public async Task<ActionResult<List<QuizQuestionDto>>> GetQuestions([FromQuery] int count = 15)
    {
        if (count <= 0) count = 15;
        if (count > 50) count = 50;

        var all = await _context.QuizQuestions.ToListAsync();
        var picked = all
            .OrderBy(_ => Guid.NewGuid()) // shuffle in-memory
            .Take(count)
            .Select(q => new QuizQuestionDto(q.Id.ToString(), q.Question, q.Options, q.AnswerIdx))
            .ToList();

        return Ok(picked);
    }
}
