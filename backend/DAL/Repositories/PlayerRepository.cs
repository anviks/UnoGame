using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class PlayerRepository(UnoDbContext db) : IPlayerRepository
{
    public async Task<Player?> GetPlayerByUserAndGame(int userId, int gameId)
    {
        return await db.Players
            .Where(p => p.UserId == userId && p.GameId == gameId)
            .SingleOrDefaultAsync();
    }
}