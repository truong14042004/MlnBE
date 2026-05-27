using DigitalDetox.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDetox.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ScreenTimeLog> ScreenTimeLogs => Set<ScreenTimeLog>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScreenTimeLog>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.Day });
            e.HasIndex(x => x.Website);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
