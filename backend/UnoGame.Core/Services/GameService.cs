using System.Text.Json;
using AutoMapper;
using FluentResults;
using UnoGame.Core.DTO;
using UnoGame.Core.DTO.Drawing;
using UnoGame.Core.Entities;
using UnoGame.Core.Helpers;
using UnoGame.Core.Interfaces;
using UnoGame.Core.State;
using UnoGame.Core.State.Enums;
using PlayerType = UnoGame.Core.State.Enums.PlayerType;

namespace UnoGame.Core.Services;

public class GameService(
    IMapper mapper,
    IGameRepository gameRepository,
    IGameStore gameStore,
    IUserRepository userRepository
)
{
    public async Task<List<Game>> GetAllGames()
    {
        return await gameRepository.GetAllGames();
    }

    public async Task<Game?> GetGameDto(int id)
    {
        return await gameRepository.GetGame(id);
    }

    public async Task<GameState?> GetGameState(int id)
    {
        GameState? state = gameStore.Get(id);
        if (state != null) return state;

        Game? game = await gameRepository.GetGame(id);
        if (game == null) return null;

        state = DeserializeState(game.SerializedState) ?? throw new JsonException("Failed to deserialize game state.");
        gameStore.Set(id, state);

        return state;
    }

    public GameState GetGameStateByGame(Game game)
    {
        GameState? state = gameStore.Get(game.Id);
        if (state != null) return state;

        state = DeserializeState(game.SerializedState) ?? throw new JsonException("Failed to deserialize game state.");
        gameStore.Set(game.Id, state);

        return state;
    }

    public async Task<Result<Game>> CreateGame(string gameName, List<CreateGamePlayer> createPlayers,
        List<Card> includedCards)
    {
        var result = new Result<Game>();

        Game? existingGame = await gameRepository.GetGameByName(gameName);

        if (existingGame != null) result.WithError("Game with this name already exists.");

        if (createPlayers.Count is < 2 or > 10) result.WithError("Player count must be between 2 and 10.");

        if (createPlayers.DistinctBy(p => p.Username).ToList().Count < createPlayers.Count)
            result.WithError("Player names must be unique.");

        if (result.IsFailed) return result;

        List<Player> players = [];

        foreach (CreateGamePlayer createPlayer in createPlayers)
        {
            var player = new Player { Name = createPlayer.Username, Cards = [] };
            players.Add(player);

            if (createPlayer.Type != PlayerType.Human) continue;
            User? user = await userRepository.GetUserByName(createPlayer.Username);
            if (user != null) player.UserId = user.Id;
        }

        var drawPile = CardHelpers.DefaultCards
            .Where(includedCards.ContainsSimilar)
            .ToList();

        var state = new GameState
        {
            Players = players,
            DrawPile = drawPile
        };

        state.ShuffleDrawPile();
        state.DealCards();
        if (state.DrawPile.All(card => card.Value == CardValue.WildDrawFour))
            throw new InvalidOperationException("No valid cards in the draw pile to start the game.");

        Card firstCard = state.DrawCard()!;
        while (firstCard.Value == CardValue.WildDrawFour)
        {
            state.DrawPile.InsertRandomly(firstCard);
            firstCard = state.DrawCard()!;
        }

        state.DiscardPile.Add(firstCard);
        state.CurrentColor = firstCard.Color;
        state.CurrentValue = firstCard.Value;

        switch (firstCard.Value)
        {
            case CardValue.Reverse:
                state.IsReversed = true;
                state.EndTurn();
                break;
            case CardValue.DrawTwo:
                state.PendingPenalty = new PendingPenalty { PlayerName = state.CurrentPlayer.Name, CardCount = 2 };
                state.EndTurn();
                break;
            case CardValue.Skip:
                state.EndTurn();
                break;
            case CardValue.Wild:
                state.CurrentColor = null;
                state.CurrentValue = null;
                break;
        }

        var game = new Game
        {
            Name = gameName,
            SerializedState = SerializeState(state)
        };

        game.CreatedAt = game.UpdatedAt = DateTime.UtcNow;
        Game createdGame = await gameRepository.CreateGame(game);

        return Result.Ok(createdGame);
    }

    /**
     * Makes the player, whose turn it currently is, play the specified card.
     * This method doesn't perform any validation (whether the player has or can play the specified card),
     * that's the responsibility of the caller.
     */
    private void PlayCard(GameState state, Player player, Card card)
    {
        player.Cards.Remove(card);
        state.DiscardPile.Insert(0, card);

        switch (card.Value)
        {
            case CardValue.Reverse:
                if (state.Players.Count > 2)
                    state.IsReversed = !state.IsReversed;
                else
                    state.EndTurn();

                break;
            case CardValue.Skip:
                state.EndTurn();
                break;
            case CardValue.DrawTwo:
                state.PendingPenalty = new PendingPenalty { PlayerName = state.NextPlayer.Name, CardCount = 2 };
                break;
            case CardValue.WildDrawFour:
                state.PendingPenalty = new PendingPenalty { PlayerName = state.NextPlayer.Name, CardCount = 4 };
                break;
        }
    }

    public async Task<Result> TryPlayCard(
        int gameId,
        Player player,
        Card card,
        CardColor? chosenColor = null
    )
    {
        GameState state = await GetGameState(gameId) ??
                          throw new ArgumentException($"Game with ID {gameId} not found.", nameof(gameId));

        if (state.WinnerIndex != null) return Result.Fail(GameErrorCodes.GameAlreadyEnded);
        if (state.CurrentPlayer != player) return Result.Fail(GameErrorCodes.NotYourTurn);
        if (state.PendingPenalty?.PlayerName == player.Name)
            return Result.Fail(GameErrorCodes.NotAllowedToPlayDuringPenalty);
        if (player.PendingDrawnCard != null && card != player.PendingDrawnCard)
            return Result.Fail(GameErrorCodes.InvalidCardAfterDraw);
        if (player.PendingDrawnCard == null && !state.CanPlayerPlayCard(player, card))
            return Result.Fail(GameErrorCodes.InvalidCard);

        PlayCard(state, player, card);

        if (card.Color == CardColor.Wild)
        {
            if (chosenColor == null)
                throw new ArgumentNullException(nameof(chosenColor), "Color must be specified for Wild cards.");
            state.CurrentColor = chosenColor.Value;
            state.CurrentValue = null;
        }
        else
        {
            state.CurrentColor = card.Color;
            state.CurrentValue = card.Value;
        }

        state.EndTurn();

        if (player.Cards.Count == 0) state.Winner = player;

        await gameRepository.UpdateGame(gameId, SerializeState(state));

        return Result.Ok();
    }

    public async Task<Result<DrawResult>> TryDrawCard(int gameId, Player player)
    {
        GameState state = await GetGameState(gameId) ??
                          throw new ArgumentException($"Game with ID {gameId} not found.", nameof(gameId));

        if (state.WinnerIndex != null) return Result.Fail(GameErrorCodes.GameAlreadyEnded);
        if (state.CurrentPlayer != player) return Result.Fail(GameErrorCodes.NotYourTurn);
        if (player.PendingDrawnCard != null) return Result.Fail(GameErrorCodes.NotAllowedToDrawTwice);

        if (state.PendingPenalty?.PlayerName == player.Name)
        {
            var cardCount = state.PendingPenalty.CardCount;
            var cards = state.DrawCardsForPlayer(player, cardCount, out var reshuffleIndex);
            state.PendingPenalty = null;
            state.EndTurn();

            await gameRepository.UpdateGame(gameId, SerializeState(state));
            return Result.Ok(new DrawResult { DrawnCards = cards, Requested = cardCount, ReshuffleIndex = reshuffleIndex });
        }

        if (!state.TryDrawCardForPlayer(player, out DrawnCard drawnCard, out var reshuffled))
            return Result.Ok(new DrawResult { Requested = 1 });

        // TODO: Don't automatically end turn if card isn't playable, otherwise other players will know if it's playable or not
        if (state.IsCardPlayable(player, drawnCard.Card)) player.PendingDrawnCard = drawnCard.Card;
        else state.EndTurn();

        await gameRepository.UpdateGame(gameId, SerializeState(state));

        return Result.Ok(new DrawResult
        {
            DrawnCards = [drawnCard],
            Requested = 1,
            ReshuffleIndex = reshuffled ? 1 : null
        });
    }

    public async Task<Result> TryEndTurn(int gameId, Player player)
    {
        GameState state = await GetGameState(gameId) ??
                          throw new ArgumentException($"Game with ID {gameId} not found.", nameof(gameId));

        if (state.WinnerIndex != null) return Result.Fail(GameErrorCodes.GameAlreadyEnded);
        if (state.CurrentPlayer != player) return Result.Fail(GameErrorCodes.NotYourTurn);
        if (player.PendingDrawnCard == null) return Result.Fail(GameErrorCodes.MustPlayOrDrawCard);

        player.PendingDrawnCard = null;
        state.EndTurn();

        await gameRepository.UpdateGame(gameId, SerializeState(state));

        return Result.Ok();
    }

    public async Task DeleteGame(int id)
    {
        gameStore.Remove(id);
        await gameRepository.DeleteGame(id);
    }

    private static GameState? DeserializeState(string state)
    {
        return JsonSerializer.Deserialize<GameState>(state);
    }

    private static string SerializeState(GameState state)
    {
        return JsonSerializer.Serialize(state);
    }
}