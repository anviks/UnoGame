using FluentResults;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;
using PlayerType = UnoGame.Core.Entities.Enums.PlayerType;

namespace UnoGame.Core.Services;

public class GameService(IGameRepository gameRepository, IUserRepository userRepository)
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

    public async Task DeleteGame(int id)
    {
        await gameRepository.DeleteGame(id);
    }
}