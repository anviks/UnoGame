using System.Text.Json;
using DAL.Entities;
using Helpers;

namespace DAL.Repositories;

public class GameRepositoryJson : IGameRepository
{
    private readonly string _saveDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnoGame",
        "saves"
    );
    private static GameRepositoryJson? _instance;

    public static GameRepositoryJson Instance
    {
        get { return _instance ??= new GameRepositoryJson(); }
    }

    private GameRepositoryJson()
    {
        if (!Directory.Exists(_saveDir))
        {
            Directory.CreateDirectory(_saveDir);
        }
    }

    public void SaveGame(GameEntity state)
    {
        var fileName = Path.Combine(_saveDir, Path.ChangeExtension(state.Id.ToString(), ".json"));
        var jsonString = JsonSerializer.Serialize(state, JsonHelper.Options);
        File.WriteAllText(fileName, jsonString);
    }

    public void DeleteGame(int id)
    {
        File.Delete(Path.Combine(_saveDir, Path.ChangeExtension(id.ToString(), ".json")));
    }

    public GameEntity? LoadGame(int id)
    {
        var savePath = Path.Combine(
            _saveDir, Path.ChangeExtension(id.ToString(), ".json")
        );

        return File.Exists(savePath)
            ? FileToGameState(savePath, new FileInfo(savePath))
            : null;
    }

    public List<GameEntity> FetchAllGames()
    {
        var dirInfo = new DirectoryInfo(_saveDir);
        var files = dirInfo.GetFiles("*.json").ToArray();

        var games = new List<GameEntity>();

        foreach (var file in files)
        {
            var savePath = Path.Combine(_saveDir, file.Name);
            var gameState = FileToGameState(savePath, file);
            games.Add(gameState);
        }

        return games;
    }

    private static GameEntity FileToGameState(string savePath, FileSystemInfo fileInfo)
    {
        var jsonString = File.ReadAllText(savePath);
        var gameState = JsonSerializer.Deserialize<GameEntity>(jsonString, JsonHelper.Options)
                        ?? throw new JsonException($"Couldn't deserialize save file at \"{savePath}\"");
        gameState.CreatedAt = fileInfo.CreationTime;
        gameState.UpdatedAt = fileInfo.LastWriteTime;
        return gameState;
    }
}