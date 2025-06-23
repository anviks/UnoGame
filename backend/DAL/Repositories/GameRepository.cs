using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class GameRepository(UnoDbContext db) : IGameRepository
{
    public async Task<List<Game>> GetAllGames()
    {
        return await db.Games
            .Include(g => g.Players)
            .ToListAsync();
    }

    public async Task<Game?> GetGame(int id)
    {
        return await db.Games
            .Include(g => g.Players)
            .ThenInclude(p => p.PlayerCards)
            .ThenInclude(pc => pc.Card)
            .Include(g => g.PileCards)
            .ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Game?> GetGameByName(string name)
    {
        return await db.Games.FirstOrDefaultAsync(g => g.Name == name);
    }

    public async Task<Game> CreateGame(Game game)
    {
        var result = await db.Games.AddAsync(game);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteGame(int id)
    {
        Game? firstOrDefault = await db.Games.FirstOrDefaultAsync(entity => entity.Id == id);
        if (firstOrDefault == null) return;
        db.Games.Remove(firstOrDefault);
        await db.SaveChangesAsync();
    }
}