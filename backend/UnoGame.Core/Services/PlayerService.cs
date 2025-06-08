using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace UnoGame.Core.Services;

public class PlayerService(IPlayerRepository playerRepository)
{
    public async Task<Player?> GetPlayerByUserAndGame(int userId, int gameId)
    {
        return await playerRepository.GetPlayerByUserAndGame(userId, gameId);
    }
}