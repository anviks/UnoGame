using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<Player?> GetPlayer(int playerId);
    public Task<Player?> GetPlayerByUserAndGame(int userId, int gameId);
    public Task<Card?> FindCard(Player player, CardColor color, CardValue value);
    public Task<Card?> RemoveCard(int playerId, CardColor color, CardValue value);
}