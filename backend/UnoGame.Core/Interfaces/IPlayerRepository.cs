using UnoGame.Core.Entities;

namespace UnoGame.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<Player?> GetPlayerByUserAndGame(int userId, int gameId);
}