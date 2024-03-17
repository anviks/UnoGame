using System.Text.Json;
using System.Text.Json.Serialization;
using DAL.Context;
using Domain;
using Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using UnoEngine;

namespace DAL;

public class GameStorageDb: IGameStorage
{
    private static GameStorageDb? _instance;
    private readonly UnoDbContext _db;
    public readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnoGame",
        "saves.db");

    private GameStorageDb()
    {
        var myLoggerFactory =
            LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
                    // .AddConsole()
                    ;
            });

        var contextOptions = new DbContextOptionsBuilder<UnoDbContext>()
            // .UseLoggerFactory(myLoggerFactory)
            .UseSqlite($"Data Source={SavePath};Cache=Shared")
            .EnableDetailedErrors()
            // .EnableSensitiveDataLogging()
            .Options;

        _db = new UnoDbContext(contextOptions);

        _db.Database.Migrate();
    }

    public GameStorageDb(UnoDbContext db)
    {
        _db = db;
    }

    public static GameStorageDb Instance
    {
        get { return _instance ??= new GameStorageDb(); }
    }

    public void SaveGame(GameState state)
    {
        var gameStateEntity = GetGameStateEntity(state.Id);

        if (gameStateEntity == null)
        {
            gameStateEntity = new GameStateEntity
            {
                Id = state.Id,
                GameName = state.GameName,
                State = JsonSerializer.Serialize(state, JsonHelper.Options)
            };

            _db.Games.Add(gameStateEntity);
        }
        else
        {
            gameStateEntity.UpdatedAt = DateTime.Now;
            gameStateEntity.State = JsonSerializer.Serialize(state, JsonHelper.Options);
        }

        _db.SaveChanges();
    }


    public void DeleteGame(Guid id)
    {
        _db.Games.Remove(GetGameStateEntity(id) ?? throw new ArgumentException($"Error on deleting GameState {id}"));
        _db.SaveChanges();
    }

    public GameState? LoadGame(Guid id)
    {
        var gameStateEntity = GetGameStateEntity(id);
        return gameStateEntity is null ? null : EntityToGameState(gameStateEntity);
    }

    private GameStateEntity? GetGameStateEntity(Guid id)
    {
        return _db.Games.FirstOrDefault(entity => entity.Id == id);
    }

    public List<GameState> FetchAllGames()
    {
        return _db.Games
            .Select(EntityToGameState)
            .ToList();
    }

    private static GameState EntityToGameState(GameStateEntity entity)
    {
        var state = JsonSerializer.Deserialize<GameState>(entity.State, JsonHelper.Options)
                    ?? throw new JsonException($"Failed parsing entity to GameState (id: {entity.Id})");
        state.CreatedAt = entity.CreatedAt;
        state.UpdatedAt = entity.UpdatedAt;
        return state;
    }
}
