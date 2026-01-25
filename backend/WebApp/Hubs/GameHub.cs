using System.Collections.Concurrent;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using UnoGame.Core.State;
using UnoGame.Core.State.Enums;

namespace WebApp.Hubs;

[Authorize]
public class GameHub(
    UserService userService,
    GameService gameService
) : Hub
{
    private static readonly ConcurrentDictionary<string, (int GameId, Player Player)> Connections = new();

    public override async Task OnConnectedAsync()
    {
        User? user = await userService.GetCurrentUser();
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found. Please log in.");
            Context.Abort();
            return;
        }

        HttpContext httpContext = Context.GetHttpContext()!;
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

        GameState? gameState = await gameService.GetGameState(gameId);

        if (gameState == null)
        {
            await Clients.Caller.SendAsync("Error", "Game not found.");
            Context.Abort();
            return;
        }

        Player? player = gameState.Players.Find(player => player.UserId == user.Id);

        if (player == null)
        {
            await Clients.Caller.SendAsync("Error", "You are not a player in this game.");
            Context.Abort();
            return;
        }

        Connections[Context.ConnectionId] = (gameId, player);

        await Groups.AddToGroupAsync(Context.ConnectionId, gameIdString);
        await Clients.Group(gameIdString).SendAsync("PlayerJoined", user.Username);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Connections.TryRemove(Context.ConnectionId, out (int GameId, Player Player) connection);
        await Clients.Group(connection.GameId.ToString())
            .SendAsync("PlayerLeft", connection.Player.Name, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<object> PlayCard(Card card, CardColor? chosenColor)
    {
        (var gameId, Player player) = Connections[Context.ConnectionId];
        Result playResult = await gameService.TryPlayCard(gameId, player, card, chosenColor);

        if (!playResult.IsSuccess) return new { Success = false, Error = playResult.Errors.First().Message };

        await Clients.Group(gameId.ToString())
            .SendAsync(
                "CardPlayed",
                player,
                card,
                chosenColor
            );
        return new { Success = true };
    }

    public async Task<object> DrawCard()
    {
        (var gameId, Player player) = Connections[Context.ConnectionId];
        var drawResult = await gameService.TryDrawCard(gameId, player);

        if (drawResult.IsFailed) return new { Success = false, Error = drawResult.Errors.First().Message };

        await Clients.OthersInGroup(gameId.ToString())
            .SendAsync(
                "CardDrawnOpponent",
                player,
                drawResult.Value.Count
            );

        await Clients.Caller
            .SendAsync(
                "CardDrawnSelf",
                drawResult.Value
            );

        return new { Success = true };
    }

    public async Task<object> EndTurn()
    {
        (var gameId, Player player) = Connections[Context.ConnectionId];
        Result endTurnResult = await gameService.TryEndTurn(gameId, player);

        if (!endTurnResult.IsSuccess) return new { Success = false, Error = endTurnResult.Errors.First().Message };

        await Clients.Group(gameId.ToString())
            .SendAsync(
                "TurnEnded",
                player
            );
        return new { Success = true };
    }
}