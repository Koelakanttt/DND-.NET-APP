using Microsoft.EntityFrameworkCore;
using TableTop.Core.Entities;

namespace TableTop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(room =>
        {
            room.Property(r => r.Name).HasMaxLength(100);
            room.Property(r => r.JoinCode).HasMaxLength(8);
            room.HasIndex(r => r.JoinCode).IsUnique();
        });
    }
}