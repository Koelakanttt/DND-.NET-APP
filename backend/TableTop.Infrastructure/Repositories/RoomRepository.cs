using Microsoft.EntityFrameworkCore;
using TableTop.Core.Entities;
using TableTop.Core.Interfaces;
using TableTop.Infrastructure.Data;

namespace TableTop.Infrastructure.Repositories;

public class RoomRepository(AppDbContext db) : IRoomRepository
{
    public Task<Room?> GetByJoinCodeAsync(string joinCode, CancellationToken ct = default)
        => db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.JoinCode == joinCode, ct);

    public Task<List<Room>> GetAllAsync(CancellationToken ct = default)
        => db.Rooms.AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(Room room, CancellationToken ct = default)
    {
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> JoinCodeExistsAsync(string joinCode, CancellationToken ct = default)
        => db.Rooms.AnyAsync(r => r.JoinCode == joinCode, ct);
}