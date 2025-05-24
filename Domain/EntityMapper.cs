using DAL.Entities;
using DAL.Entities.Cards;
using DAL.Entities.Players;

namespace Domain;

public static class EntityMapper
{
    public static Game ToDomain(GameEntity entity)
    {
        return new Game
        {
            Id = entity.Id,
            History = entity.History,
            Deck = ExtractCardDeck(entity.Cards ?? []),
            DiscardPile = ExtractDiscardPile(entity.Cards ?? []),
            Players = entity.Players.Select(ToDomain).ToList(),
            CurrentColor = entity.CurrentColor,
            CurrentValue = entity.CurrentValue,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            GameName = entity.GameName,
            CurrentPlayerIndex = entity.CurrentPlayerIndex,
            IsReversed = entity.IsReversed,
        };
    }

    public static GameEntity ToEntity(Game game)
    {
        List<CardEntity> cards = [];

        cards.AddRange(game.Deck.Cards.Select((card, index) => new CardEntity
        {
            Id = card.Id,
            Color = card.Color,
            Value = card.Value,
            Location = CardLocation.Deck,
            GameId = game.Id,
            Position = index,
        }));

        cards.AddRange(game.DiscardPile.Select((card, index) => new CardEntity
        {
            Id = card.Id,
            Color = card.Color,
            Value = card.Value,
            Location = CardLocation.DiscardPile,
            GameId = game.Id,
            Position = index,
        }));

        foreach (Player player in game.Players)
        {
            cards.AddRange(player.Hand.Cards.Select((card, index) =>
            {
                var cardEntity = new CardEntity
                {
                    Id = card.Id,
                    Color = card.Color,
                    Value = card.Value,
                    Location = CardLocation.Hand,
                    GameId = game.Id,
                    Position = index,
                };
                if (player.Id != 0) cardEntity.PlayerId = player.Id;
                return cardEntity;
            }));
        }

        return new GameEntity
        {
            Id = game.Id,
            History = game.History,
            Cards = cards,
            Players = game.Players.Select(ToEntity).ToList(),
            CurrentColor = game.CurrentColor,
            CurrentValue = game.CurrentValue,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
            GameName = game.GameName,
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            IsReversed = game.IsReversed,
        };
    }


    public static Player ToDomain(PlayerEntity entity)
    {
        return new Player
        {
            Id = entity.Id,
            Hand = ExtractCardHand(entity.Hand),
            Type = entity.Type,
            Name = entity.Name,
            UserId = entity.UserId,
            SaidUno = entity.SaidUno,
        };
    }

    public static PlayerEntity ToEntity(Player player)
    {
        return new PlayerEntity
        {
            Id = player.Id,
            Type = player.Type,
            Name = player.Name,
            UserId = player.UserId,
            SaidUno = player.SaidUno,
            Hand = player.Hand.Cards.Select((card, index) => new CardEntity { Color = card.Color, Value = card.Value, Location = CardLocation.Hand, Position = index }).ToList()
        };
    }

    public static User ToDomain(UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            Email = entity.Email,
            Name = entity.Name,
            Token = entity.Token,
        };
    }

    public static UserEntity ToEntity(User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            Email = user.Email,
            Name = user.Name,
            Token = user.Token,
        };
    }

    public static Card ToDomain(CardEntity entity)
    {
        return new Card
        {
            Id = entity.Id,
            Color = entity.Color,
            Value = entity.Value,
        };
    }

    public static CardHand ExtractCardHand(List<CardEntity> cards)
    {
        return new CardHand(
            cards
                .Where(c => c.Location == CardLocation.Hand)
                .OrderBy(c => c.Position)
                .Select(ToDomain)
                .ToList()
        );
    }

    public static CardDeck ExtractCardDeck(List<CardEntity> cards)
    {
        return new CardDeck(
            cards
                .Where(c => c.Location == CardLocation.Deck)
                .OrderBy(c => c.Position)
                .Select(ToDomain)
                .ToList()
        );
    }

    public static List<Card> ExtractDiscardPile(List<CardEntity> cards)
    {
        return cards
            .Where(c => c.Location == CardLocation.DiscardPile)
            .OrderBy(c => c.Position)
            .Select(ToDomain)
            .ToList();
    }
}