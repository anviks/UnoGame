using Domain;
using UnoEngine;

namespace DAL;

public interface IGameStorage
{
    void SaveGame(GameState state);
    void DeleteGame(Guid id);
    GameState? LoadGame(Guid id);
    List<GameState> FetchAllGames();
}