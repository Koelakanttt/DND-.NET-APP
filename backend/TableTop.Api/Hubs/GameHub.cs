using Microsoft.AspNetCore.SignalR;
using TableTop.Core.Entities;
using TableTop.Core.Interfaces;
using TableTop.Core.Services;
using TableTop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace TableTop.Api.Hubs;

public class GameHub(
    AppDbContext db,
    DiceService diceService,
    IChatMessageRepository chatMessages,
    RoomService roomService) : Hub
{
    public async Task JoinRoom(string joinCode, string playerName)
    {
        var room = await roomService.FindByJoinCodeAsync(joinCode);
        if (room is null)
        {
            await Clients.Caller.SendAsync("MessageReceived", "⚠️", "Комната не найдена", DateTimeOffset.UtcNow);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);
        Context.Items["room"] = joinCode;
        Context.Items["roomId"] = room.Id;
        Context.Items["name"] = playerName;

        var history = await chatMessages.GetLastAsync(room.Id, 50);
        await Clients.Caller.SendAsync("History",
            history.Select(m => new { playerName = m.PlayerName, text = m.Text, sentAt = m.SentAt }));

        await Clients.OthersInGroup(joinCode).SendAsync("PlayerJoined", playerName);
        var tokens = await db.MapTokens.AsNoTracking().Where(t => t.RoomId == room.Id).ToListAsync();
        await Clients.Caller.SendAsync("MapState", room.MapUrl, tokens.Select(ToDto));
    }

    public async Task SendMessage(string text)
    {
        if (Context.Items["room"] is not string joinCode ||
            Context.Items["roomId"] is not Guid roomId ||
            Context.Items["name"] is not string playerName)
            return;

        if (text.StartsWith("/roll ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleRoll(joinCode, playerName, text["/roll ".Length..]);
            return;
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            PlayerName = playerName,
            Text = text,
            SentAt = DateTimeOffset.UtcNow
        };
        await chatMessages.AddAsync(message);

        await Clients.Group(joinCode)
            .SendAsync("MessageReceived", playerName, text, message.SentAt);
    }
    public async Task AddToken(string name, string color)
{
    if (Context.Items["room"] is not string joinCode ||
        Context.Items["roomId"] is not Guid roomId) return;

    var token = new MapToken
    {
        Id = Guid.NewGuid(), RoomId = roomId,
        Name = name.Trim(), Color = color,
        X = 50, Y = 50
    };
    db.MapTokens.Add(token);
    await db.SaveChangesAsync();

    await Clients.Group(joinCode).SendAsync("TokenAdded", ToDto(token));
}

// Живое перетаскивание: только рассылка, БЕЗ записи в базу
public async Task MoveToken(Guid tokenId, double x, double y)
{
    if (Context.Items["room"] is not string joinCode) return;
    await Clients.OthersInGroup(joinCode).SendAsync("TokenMoved", tokenId, x, y);
}

// Фишку отпустили: фиксируем позицию в базе
public async Task DropToken(Guid tokenId, double x, double y)
{
    if (Context.Items["room"] is not string joinCode) return;

    var token = await db.MapTokens.FindAsync(tokenId);
    if (token is null) return;

    token.X = Math.Clamp(x, 0, 100);
    token.Y = Math.Clamp(y, 0, 100);
    await db.SaveChangesAsync();

    await Clients.OthersInGroup(joinCode).SendAsync("TokenMoved", tokenId, token.X, token.Y);
}

public async Task RemoveToken(Guid tokenId)
{
    if (Context.Items["room"] is not string joinCode) return;
    await db.MapTokens.Where(t => t.Id == tokenId).ExecuteDeleteAsync();
    await Clients.Group(joinCode).SendAsync("TokenRemoved", tokenId);
}

private static object ToDto(MapToken t) => new { id = t.Id, name = t.Name, color = t.Color, x = t.X, y = t.Y };

    private async Task HandleRoll(string joinCode, string playerName, string notation)
    {
        try
        {
            var result = diceService.Roll(notation);
            await Clients.Group(joinCode).SendAsync("DiceRolled", playerName, new
            {
                notation = notation.Trim(),
                rolls = result.Rolls,
                modifier = result.Modifier,
                total = result.Total
            });
        }
        catch (FormatException ex)
        {
            await Clients.Caller.SendAsync("MessageReceived", "🎲", ex.Message, DateTimeOffset.UtcNow);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items["room"] is string joinCode &&
            Context.Items["name"] is string playerName)
        {
            await Clients.OthersInGroup(joinCode)
                .SendAsync("PlayerLeft", playerName);
        }

        await base.OnDisconnectedAsync(exception);
    }
}