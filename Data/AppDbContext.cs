using DigitalDetox.Api.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace DigitalDetox.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ScreenTimeLog> ScreenTimeLogs => Set<ScreenTimeLog>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScreenTimeLog>().ToCollection("ScreenTimeLogs");
        modelBuilder.Entity<User>().ToCollection("Users");
    }
}
