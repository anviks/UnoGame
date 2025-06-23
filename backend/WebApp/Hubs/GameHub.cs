using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;
using UnoGame.Core.Services;
using WebApp.DTO;

namespace WebApp.Hubs;

[Authorize]
public class GameHub(
    UserService userService,
    GameService gameService,
    PlayerService playerService,
    IMapper mapper
) : Hub
{
    private static readonly ConcurrentDictionary<string, Player> Connections = new();

    public override async Task OnConnectedAsync()
    {
        User? user = await userService.GetCurrentUser();
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found. Please log in.");
            Context.Abort();
            return;
        }

        var httpContext = Context.GetHttpContext()!;
        var gameIdString = httpContext.Request.Query["gameId"].ToString();

        if (string.IsNullOrEmpty(gameIdString))
        {
            await Clients.Caller.SendAsync("Error", "Game ID is required.");
            Context.Abort();
            return;
        }

        if (!int.TryParse(gameIdString, out var gameId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid Game ID format.");
            Context.Abort();
            return;
        }

        var game = await gameService.GetGame(gameId);

        if (game == null)
        {
            await Clients.Caller.SendAsync("Error", "Game not found.");
            Context.Abort();
            return;
        }

        var player = await playerService.GetPlayerByUserAndGame(user.Id, gameId);

        if (player == null)
        {
            await Clients.Caller.SendAsync("Error", "You are not a player in this game.");
            Context.Abort();
            return;
        }

        Connections[Context.ConnectionId] = player;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameIdString);
        await Clients.Group(gameIdString).SendAsync("PlayerJoined", user.Name);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Connections.TryRemove(Context.ConnectionId, out Player? player);
        await Clients.Group(player!.GameId.ToString()).SendAsync("PlayerLeft", player.Name, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task PlayCard(CardDto card, CardColor? chosenColor)
    {
        Player player = Connections[Context.ConnectionId];

        if (await gameService.TryPlayCard(player.GameId, player.Id, card.Color, card.Value, chosenColor))
        {
            await Clients.Group(player.GameId.ToString())
                .SendAsync(
                    "CardPlayed",
                    mapper.Map<PlayerDto>(player),
                    mapper.Map<CardDto>(card),
                    chosenColor
                );
        }
        else
        {
            throw new HubException("You cannot play this card at the moment.");
        }
    }
}