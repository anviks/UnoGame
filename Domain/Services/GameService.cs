using DAL.Entities;
using DAL.Entities.Players;
using DAL.Repositories;
using FluentResults;

namespace Domain.Services;

public class GameService(GameRepository gameRepository, UserRepository userRepository, PlayerRepository playerRepository)
{
    public async Task<List<Game>> GetAllGames()
    {
        return (await gameRepository.GetAllGames()).Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<Game?> GetGame(int id)
    {
        GameEntity? gameEntity = await gameRepository.GetGame(id);
        return gameEntity != null ? EntityMapper.ToDomain(gameEntity) : null;
    }

    public async Task<Result<Game>> CreateGame(Game game)
    {
        var result = new Result<Game>();

        GameEntity? existingGame = await gameRepository.GetGameByName(game.GameName);
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
            player.Hand = new CardHand([]);

            if (player.Type != PlayerType.Human) continue;
            UserEntity? user = await userRepository.GetUserByName(player.Name);
            if (user != null) player.UserId = user.Id;
        }

        game.CreatedAt = game.UpdatedAt = DateTime.UtcNow;

        game.DealCards();

        GameEntity gameEntity = EntityMapper.ToEntity(game);
        GameEntity savedGame = await gameRepository.CreateGame(gameEntity);

        return Result.Ok(EntityMapper.ToDomain(savedGame));
    }

    public async Task DeleteGame(int id)
    {
        await gameRepository.DeleteGame(id);
    }
}