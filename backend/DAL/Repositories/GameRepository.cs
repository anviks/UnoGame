using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class GameRepository(UnoDbContext db) : IGameRepository
{
    public async Task<List<Game>> GetAllGames()
    {
        return await db.Games.ToListAsync();
    }

    public async Task<Game?> GetGame(int id)
    {
        return await db.Games.FindAsync(id);
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

    public async Task<bool> UpdateGame(int id, string gameState)
    {
        Game? game = await db.Games.FindAsync(id);
        if (game == null) return false;
        game.SerializedState = gameState;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task DeleteGame(int id)
    {
        Game? firstOrDefault = await db.Games.FindAsync(id);
        if (firstOrDefault == null) return;
        db.Games.Remove(firstOrDefault);
        await db.SaveChangesAsync();
    }
}