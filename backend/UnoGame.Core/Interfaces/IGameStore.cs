using UnoGame.Core.State;

namespace UnoGame.Core.Interfaces;

public interface IGameStore
{
    GameState? Get(int gameId);
    void Set(int gameId, GameState state);
    void Remove(int gameId);
}