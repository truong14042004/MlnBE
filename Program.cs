using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using DigitalDetox.Api.Data;
using DigitalDetox.Api.Models;
using DigitalDetox.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MongoDB cho dev và prod
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "mongodb://localhost:27017";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMongoDB(connectionString, "DigitalDetox"));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<TokenService>();

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_ME_dev_only_super_secret_key_min_32_chars_long_0123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DigitalDetoxApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DigitalDetoxClient";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

// Rate limiting on auth endpoints to prevent brute-force.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
    });
});

const string CorsPolicy = "DigitalDetoxCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Áp dụng migrations còn thiếu khi khởi động (an toàn cho dev và prod nhỏ).
// MongoDB tự động khởi tạo database khi có dữ liệu, không cần migrations.

// Seed bộ câu hỏi (nguồn: question.pdf) vào MongoDB nếu collection còn trống.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (!await db.QuizQuestions.AnyAsync())
        {
            var seedPath = Path.Combine(app.Environment.ContentRootPath, "quiz_questions.json");
            if (File.Exists(seedPath))
            {
                var json = await File.ReadAllTextAsync(seedPath);
                var items = JsonSerializer.Deserialize<List<QuizSeedItem>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                var questions = items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Question) && i.Options is { Count: >= 2 })
                    .Select(i => new QuizQuestion
                    {
                        Id = Guid.NewGuid(),
                        Question = i.Question!,
                        Options = i.Options!,
                        AnswerIdx = i.AnswerIdx
                    })
                    .ToList();

                if (questions.Count > 0)
                {
                    db.QuizQuestions.AddRange(questions);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"[QuizSeed] Seeded {questions.Count} questions.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[QuizSeed] Skipped seeding: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { name = "Digital Detox API", status = "ok" }));

app.Run();

// DTO dùng để đọc file seed quiz_questions.json.
record QuizSeedItem
{
    public string? Question { get; set; }
    public List<string>? Options { get; set; }
    public int AnswerIdx { get; set; }
}
