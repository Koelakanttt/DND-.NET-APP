using Microsoft.EntityFrameworkCore;
using TableTop.Core.Entities;

namespace TableTop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<MapToken> MapTokens => Set<MapToken>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(room =>
        {
            room.Property(r => r.Name).HasMaxLength(100);
            room.Property(r => r.JoinCode).HasMaxLength(8);
            room.HasIndex(r => r.JoinCode).IsUnique();
        });
        modelBuilder.Entity<ChatMessage>(msg =>
        {
            msg.Property(m => m.PlayerName).HasMaxLength(50);
            msg.Property(m => m.Text).HasMaxLength(1000);
            msg.HasIndex(m => new { m.RoomId, m.SentAt });
            msg.HasOne(m => m.Room)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<MapToken>(token =>
        {
            token.Property(t => t.ImageUrl).HasMaxLength(300);
            token.Property(t => t.Name).HasMaxLength(50);
            token.Property(t => t.Color).HasMaxLength(9);
            token.HasIndex(t => t.RoomId);
            token.HasOne(t => t.Room).WithMany().HasForeignKey(t => t.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Room>(room =>
        {
            room.Property(r => r.MapUrl).HasMaxLength(300);
        });
    }
}