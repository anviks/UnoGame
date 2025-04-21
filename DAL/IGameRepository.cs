using Domain;
using UnoEngine;

namespace DAL;

public interface IGameRepository
{
    void SaveGame(GameState state);
    void DeleteGame(int id);
    GameState? LoadGame(int id);
    List<GameState> FetchAllGames();
}