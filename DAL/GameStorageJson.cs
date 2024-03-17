using static System.Web.HttpUtility;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;
using Helpers;
using UnoEngine;

namespace DAL;

public class GameStorageJson : IGameStorage
{
    private readonly string _saveDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnoGame",
        "saves"
    );
    private static GameStorageJson? _instance;

    public static GameStorageJson Instance
    {
        get { return _instance ??= new GameStorageJson(); }
    }

    private GameStorageJson()
    {
        if (!Directory.Exists(_saveDir))
        {
            Directory.CreateDirectory(_saveDir);
        }
    }

    public void SaveGame(GameState state)
    {
        var fileName = Path.Combine(_saveDir, Path.ChangeExtension(state.Id.ToString(), ".json"));
        var jsonString = JsonSerializer.Serialize(state, JsonHelper.Options);
        File.WriteAllText(fileName, jsonString);
    }

    public void DeleteGame(Guid id)
    {
        File.Delete(Path.Combine(_saveDir, Path.ChangeExtension(id.ToString(), ".json")));
    }

    public GameState? LoadGame(Guid id)
    {
        var savePath = Path.Combine(
            _saveDir, Path.ChangeExtension(id.ToString(), ".json")
        );

        return File.Exists(savePath)
            ? FileToGameState(savePath, new FileInfo(savePath))
            : null;
    }

    public List<GameState> FetchAllGames()
    {
        var dirInfo = new DirectoryInfo(_saveDir);
        var files = dirInfo.GetFiles("*.json").ToArray();

        var games = new List<GameState>();

        foreach (var file in files)
        {
            var savePath = Path.Combine(_saveDir, file.Name);
            var gameState = FileToGameState(savePath, file);
            games.Add(gameState);
        }

        return games;
    }

    private static GameState FileToGameState(string savePath, FileSystemInfo fileInfo)
    {
        var jsonString = File.ReadAllText(savePath);
        var gameState = JsonSerializer.Deserialize<GameState>(jsonString, JsonHelper.Options)
                        ?? throw new JsonException($"Couldn't deserialize save file at \"{savePath}\"");
        gameState.CreatedAt = fileInfo.CreationTime;
        gameState.UpdatedAt = fileInfo.LastWriteTime;
        return gameState;
    }
}