using FluentResults;
using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;
using UnoGame.Core.Interfaces;
using PlayerType = UnoGame.Core.Entities.Enums.PlayerType;

namespace UnoGame.Core.Services;

public class GameService(
    IGameRepository gameRepository,
    IUserRepository userRepository,
    IPlayerRepository playerRepository
)
{
    public async Task<List<Game>> GetAllGames()
    {
        return await gameRepository.GetAllGames();
    }

    public async Task<Game?> GetGame(int id)
    {
        Game? game = await gameRepository.GetGame(id);
        game?.SyncPilesFromPileCards();
        return game;
    }

    public async Task<Result<Game>> CreateGame(Game game)
    {
        var result = new Result<Game>();

        Game? existingGame = await gameRepository.GetGameByName(game.Name);
        if (existingGame != null)
        {
            result.WithError("Game with this name already exists.");
        }

        if (game.Players.Count is < 2 or > 10)
        {
            result.WithError("Player count must be between 2 and 10.");
        }

        if (game.Players.DistinctBy(p => p.Name).ToList().Count < game.Players.Count)
        {
            result.WithError("Player names must be unique.");
        }

        if (result.IsFailed) return result;

        foreach (Player player in game.Players)
        {
            player.PlayerCards = [];

            if (player.Type != PlayerType.Human) continue;
            User? user = await userRepository.GetUserByName(player.Name);
            if (user != null) player.UserId = user.Id;
        }

        game.CreatedAt = game.UpdatedAt = DateTime.UtcNow;

        game.ShuffleDrawPile();
        game.DealCards();

        Game savedGame = await gameRepository.CreateGame(game);

        return Result.Ok(savedGame);
    }

    public async Task<bool> TryPlayCard(
        int gameId,
        int playerId,
        CardColor color,
        CardValue value,
        CardColor? chosenColor = null
    )
    {
        Game game = await gameRepository.GetGame(gameId)
                    ?? throw new ArgumentException($"Game with ID {gameId} not found.", nameof(gameId));
        game.EnsurePilesSynced();
        Player player = await playerRepository.GetPlayer(playerId)
                        ?? throw new ArgumentException($"Player with ID {playerId} not found.", nameof(playerId));

        if (!game.CanPlayCard(player, color, value)) return false;

        Card card = await playerRepository.FindCard(player, color, value)
                    ?? throw new ArgumentException(
                        $"Card with color {color} and value {value} not found in player's hand.", nameof(value));

        await playerRepository.RemoveCard(player.Id, card.Color, card.Value);
        game.DiscardPile.Insert(0, card);

        if (card.Color == CardColor.Wild)
        {
            if (chosenColor == null)
                throw new ArgumentNullException(nameof(chosenColor), "Color must be specified for Wild cards.");
            game.CurrentColor = chosenColor.Value;
            game.CurrentValue = null;
        }
        else
        {
            game.CurrentColor = card.Color;
            game.CurrentValue = card.Value;
        }

        await UpdateGame(game);

        return true;
    }

    private async Task UpdateGame(Game game)
    {
        game.EnsurePileCardsSynced();
        await gameRepository.SaveChangesAsync();
    }

    public async Task DeleteGame(int id)
    {
        await gameRepository.DeleteGame(id);
    }
}