using TableTop.Core.Entities;

namespace TableTop.Core.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByJoinCodeAsync(string joinCode, CancellationToken ct = default);
    Task<List<Room>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    Task<bool> JoinCodeExistsAsync(string joinCode, CancellationToken ct = default);
}