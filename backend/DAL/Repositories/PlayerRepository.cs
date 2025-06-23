using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class PlayerRepository(UnoDbContext db) : IPlayerRepository
{
    public async Task<Player?> GetPlayer(int playerId)
    {
        return await db.Players.FindAsync(playerId);
    }

    public async Task<Player?> GetPlayerByUserAndGame(int userId, int gameId)
    {
        return await db.Players
            .Where(p => p.UserId == userId && p.GameId == gameId)
            .SingleOrDefaultAsync();
    }

    private async Task<PlayerCard?> FindPlayerCard(int playerId, CardColor color, CardValue value)
    {
        return await db.PlayerCards
            .Include(playerCard => playerCard.Card)
            .Where(pc => pc.PlayerId == playerId && pc.Card.Color == color && pc.Card.Value == value)
            .FirstOrDefaultAsync();
    }

    public async Task<Card?> FindCard(Player player, CardColor color, CardValue value)
    {
        return (await FindPlayerCard(player.Id, color, value))?.Card;
    }

    public async Task<Card?> RemoveCard(int playerId, CardColor color, CardValue value)
    {
        PlayerCard? removedCard = await FindPlayerCard(playerId, color, value);
        if (removedCard == null) return null;
        db.PlayerCards.Remove(removedCard);

        return removedCard.Card;
    }
}