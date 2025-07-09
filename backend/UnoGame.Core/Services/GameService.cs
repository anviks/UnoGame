using System.Text.Json;
using FluentResults;
using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;
using UnoGame.Core.Interfaces;
using UnoGame.Core.State;
using PlayerType = UnoGame.Core.Entities.Enums.PlayerType;

namespace UnoGame.Core.Services;

public class GameService(
    IGameRepository gameRepository,
    IGameStore gameStore,
    IUserRepository userRepository
)
{
    public async Task<List<Game>> GetAllGames()
    {
        return await gameRepository.GetAllGames();
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

    public async Task<Result<GameState>> CreateGame(string gameName, GameState gameState)
    {
        var result = new Result<GameState>();

        Game? existingGame = await gameRepository.GetGameByName(gameName);

        if (existingGame != null)
        {
            result.WithError("Game with this name already exists.");
        }

        if (gameState.Players.Count is < 2 or > 10)
        {
            result.WithError("Player count must be between 2 and 10.");
        }

        if (gameState.Players.DistinctBy(p => p.Name).ToList().Count < gameState.Players.Count)
        {
            result.WithError("Player names must be unique.");
        }

        if (result.IsFailed) return result;

        foreach (Player player in gameState.Players)
        {
            player.Cards = [];

            if (player.Type != PlayerType.Human) continue;
            User? user = await userRepository.GetUserByName(player.Name);
            if (user != null) player.UserId = user.Id;
        }

        gameState.ShuffleDrawPile();
        gameState.DealCards();

        var game = new Game
        {
            Name = gameName,
            SerializedState = SerializeState(gameState),
        };

        game.CreatedAt = game.UpdatedAt = DateTime.UtcNow;
        await gameRepository.CreateGame(game);

        return Result.Ok(gameState);
    }

    /**
     * Makes the player, whose turn it currently is, play the specified card.
     * This method doesn't perform any validation (whether the player has or can play the specified card),
     * that's the responsibility of the caller.
     */
    private void PlayCurrentPlayerCard(GameState state, Card card)
    {
        Player player = state.CurrentPlayer;
        Player target;
        player.Cards.Remove(card);
        state.DiscardPile.Insert(0, card);

        switch (card.Value)
        {
            case CardValue.Reverse:
                if (state.Players.Count > 2)
                {
                    state.IsReversed = !state.IsReversed;
                    state.History.Add($"{player.Name} reversed the game");
                }
                else
                {
                    state.EndTurn();
                    state.History.Add($"{state.CurrentPlayer.Name} was skipped");
                }
                break;
            case CardValue.Skip:
                state.EndTurn();
                state.History.Add($"{state.CurrentPlayer.Name} was skipped");
                break;
            case CardValue.DrawTwo:
                state.EndTurn();
                target = state.CurrentPlayer;
                state.DrawCardsForPlayer(target, 2);
                state.History.Add($"{target.Name} drew 2 cards");  // TODO: Not true if cards have run out
                break;
            case CardValue.WildDrawFour:
                state.EndTurn();
                target = state.CurrentPlayer;
                state.DrawCardsForPlayer(target, 4);
                state.History.Add($"{target.Name} drew 4 cards");  // TODO: Not true if cards have run out
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

        if (state.WinnerIndex != null) return Result.Fail("GAME_ALREADY_FINISHED");
        if (state.CurrentPlayer != player) return Result.Fail("NOT_YOUR_TURN");
        if (player.PendingDrawnCard != null && card != player.PendingDrawnCard) return Result.Fail("INVALID_CARD_TO_PLAY_AFTER_DRAW");
        if (player.PendingDrawnCard == null && !state.CanPlayerPlayCard(player, card)) return Result.Fail("INVALID_CARD");

        PlayCurrentPlayerCard(state, card);

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

    public async Task<Result> TryDrawCard(int gameId, Player player)
    {
        GameState state = await GetGameState(gameId) ??
                          throw new ArgumentException($"Game with ID {gameId} not found.", nameof(gameId));

        if (state.WinnerIndex != null) return Result.Fail("GAME_ALREADY_ENDED");
        if (state.CurrentPlayer != player) return Result.Fail("NOT_YOUR_TURN");
        if (player.PendingDrawnCard != null) return Result.Fail("NOT_ALLOWED_TO_DRAW_TWICE");

        Card? card = state.DrawCardForPlayer(player);
        if (card == null) return Result.Fail("NO_CARDS_TO_DRAW");

        if (state.IsCardPlayable(player, card)) player.PendingDrawnCard = card;
        else state.EndTurn();

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