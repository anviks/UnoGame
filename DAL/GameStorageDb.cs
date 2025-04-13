using System.Text.Json;
using DAL.Context;
using Domain;
using Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL;

public class GameStorageDb : IGameStorage
{
    private static GameStorageDb? _instance;
    private readonly UnoDbContext _db;
    public readonly string SavePath = InitializeSavePath();

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

    private static string InitializeSavePath()
    {
        var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string savePath;

        if (baseFolder.Length > 0)
        {
            savePath = Path.Combine(
                baseFolder,
                "UnoGame",
                "saves.db");
        }
        else
        {
            savePath = Path.Combine(
                AppContext.BaseDirectory,
                "App_Data",
                "saves.db"
            );
        }

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
        }

        return savePath;
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