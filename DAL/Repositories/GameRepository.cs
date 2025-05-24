using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories;

public class GameRepository(UnoDbContext db)
{
    public async Task<List<GameEntity>> GetAllGames()
    {
        return await db.Games
            .Include(g => g.Players)
            .ToListAsync();
    }

    public async Task<GameEntity?> GetGame(int id)
    {
        return await db.Games
            .Include(g => g.Players)
            .ThenInclude(p => p.Hand)
            .Include(g => g.Cards)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GameEntity?> GetGameByName(string name)
    {
        return await db.Games.FirstOrDefaultAsync(g => g.GameName == name);
    }

    public async Task<GameEntity> CreateGame(GameEntity game)
    {
        var result = await db.Games.AddAsync(game);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteGame(int id)
    {
        GameEntity? firstOrDefault = await db.Games.FirstOrDefaultAsync(entity => entity.Id == id);
        if (firstOrDefault == null) return;
        db.Games.Remove(firstOrDefault);
        await db.SaveChangesAsync();
    }
}