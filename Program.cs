using DigitalDetox.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQL Server cho dev (LocalDB) và prod
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=DigitalDetox;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { name = "Digital Detox API", status = "ok" }));

app.Run();
