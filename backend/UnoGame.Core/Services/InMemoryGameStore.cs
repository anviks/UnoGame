using System.Collections.Concurrent;
using UnoGame.Core.Interfaces;
using UnoGame.Core.State;

namespace UnoGame.Core.Services;

public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<int, GameState> _games = new();

    public GameState? Get(int gameId) => _games.GetValueOrDefault(gameId);
    public void Set(int gameId, GameState state) => _games[gameId] = state;
    public void Remove(int gameId) => _games.TryRemove(gameId, out _);
}