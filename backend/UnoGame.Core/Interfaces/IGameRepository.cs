using UnoGame.Core.Entities;

namespace UnoGame.Core.Interfaces;

public interface IGameRepository
{
    public Task<List<Game>> GetAllGames();
    public Task<Game?> GetGame(int id);
    public Task<Game?> GetGameByName(string name);
    public Task<Game> CreateGame(Game game);
    public Task SaveChangesAsync();
    public Task DeleteGame(int id);
}