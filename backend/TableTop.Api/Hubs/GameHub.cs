using Microsoft.AspNetCore.SignalR;
using TableTop.Core.Services;

namespace TableTop.Api.Hubs;

public class GameHub(DiceService diceService) : Hub
{
    public async Task JoinRoom(string joinCode, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);

        Context.Items["room"] = joinCode;
        Context.Items["name"] = playerName;

        await Clients.OthersInGroup(joinCode)
            .SendAsync("PlayerJoined", playerName);
    }

    public async Task SendMessage(string text)
    {
        if (Context.Items["room"] is not string joinCode ||
            Context.Items["name"] is not string playerName)
            return;

        if (text.StartsWith("/roll ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleRoll(joinCode, playerName, text["/roll ".Length..]);
            return;
        }

        await Clients.Group(joinCode)
            .SendAsync("MessageReceived", playerName, text, DateTimeOffset.UtcNow);
    }

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