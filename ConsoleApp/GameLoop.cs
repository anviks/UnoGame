using System.Text.Json;
using DAL;
using Domain;
using Domain.Cards;
using Domain.Players;
using UnoEngine;

namespace ConsoleApp;

public static class GameLoop
{
    private static void Setup()
    {
        Console.Clear();
        Console.CursorVisible = true;
    }

    public static void StartNewGame(string name, List<Player> players, GameConfiguration configuration, IGameStorage storage)
    {
        Setup();
        var state = new GameState
        {
            GameName = name,
            Players = players
        };

        var engine = new Engine
        {
            State = state
        };

        engine.ApplyConfiguration(configuration);
        var controller = new GameController(engine, storage);
        controller.Setup();
        controller.Run();
    }

    public static void ContinueGame(IGameStorage storage, Guid id)
    {
        Setup();
        var state = storage.LoadGame(id) ?? throw new ArgumentException($"Couldn't find the game with the id {id}");
        var engine = new Engine
        {
            State = state
        };
        var controller = new GameController(engine, storage);
        controller.Run();
    }
}