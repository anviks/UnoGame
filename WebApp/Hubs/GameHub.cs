using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;

namespace WebApp.Hubs;

[Authorize]
public class GameHub(UserService userService) : Hub
{
    private static readonly ConcurrentDictionary<string, User> Connections = new();

    public override async Task OnConnectedAsync()
    {
        User? user = await userService.GetCurrentUser();
        if (user == null)
        {
            Context.Abort();
            return;
        }

        Connections[Context.ConnectionId] = user;
        await Clients.All.SendAsync("PlayerJoined", user.Name, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Connections.TryRemove(Context.ConnectionId, out User? user);
        await Clients.All.SendAsync("PlayerLeft", user?.Name ?? "Unknown", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task MakeMove(string player, string move)
    {
        await Clients.All.SendAsync("ReceiveMove", player, move);
    }
}