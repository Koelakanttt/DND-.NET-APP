using TableTop.Core.Entities;
using TableTop.Core.Interfaces;

namespace TableTop.Core.Services;

public class RoomService(IRoomRepository rooms)
{
    public async Task<Room> CreateAsync(string name, CancellationToken ct = default)
    {
        var joinCode = await GenerateUniqueJoinCodeAsync(ct);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = name,
            JoinCode = joinCode,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await rooms.AddAsync(room, ct);
        return room;
    }

    public Task<Room?> FindByJoinCodeAsync(string joinCode, CancellationToken ct = default)
        => rooms.GetByJoinCodeAsync(joinCode.Trim().ToUpperInvariant(), ct);

    public Task<List<Room>> GetAllAsync(CancellationToken ct = default)
        => rooms.GetAllAsync(ct);

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = GenerateJoinCode();
            if (!await rooms.JoinCodeExistsAsync(code, ct))
                return code;
        }
        throw new InvalidOperationException("Не удалось сгенерировать уникальный код комнаты.");
    }

private static readonly char[] Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789".ToCharArray();
private const int CodeLength = 6;
    private static string GenerateJoinCode()
    {
            return string.Create(CodeLength, Alphabet, (span, alphabet) =>
        {
            Random.Shared.GetItems(alphabet, span);
        });
    }
}