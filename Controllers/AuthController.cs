using DigitalDetox.Api.Data;
using DigitalDetox.Api.Models;
using DigitalDetox.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DigitalDetox.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<string> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<string>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new { message = "Tài khoản và mật khẩu không được để trống." });
        }

        var normalizedUsername = model.Username.Trim().ToLower();
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            return BadRequest(new { message = "Tài khoản này đã tồn tại." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = model.Username.Trim(),
            FullName = string.IsNullOrWhiteSpace(model.FullName) ? model.Username.Trim() : model.FullName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user.Username, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            userId = user.Id.ToString(),
            username = user.Username,
            fullName = user.FullName,
            token = _tokenService.CreateToken(user.Id.ToString(), user.Username),
            message = "Đăng ký thành công!"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new { message = "Tài khoản và mật khẩu không được để trống." });
        }

        var normalizedUsername = model.Username.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);

        if (user == null)
        {
            return BadRequest(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, model.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return BadRequest(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
        }

        return Ok(new
        {
            userId = user.Id.ToString(),
            username = user.Username,
            fullName = user.FullName,
            token = _tokenService.CreateToken(user.Id.ToString(), user.Username),
            message = "Đăng nhập thành công!"
        });
    }
}

public class AuthDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
