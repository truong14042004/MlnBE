using System.Text;
using System.Threading.RateLimiting;
using DigitalDetox.Api.Data;
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
