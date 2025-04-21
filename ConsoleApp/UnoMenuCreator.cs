using DAL;
using Domain;
using Domain.Players;
using MenuSystem;
using MenuSystem.MenuItems;

namespace ConsoleApp;

public static class UnoMenuCreator
{
    public static MenuLevel CreatePlayerMenuLevel(string gameName, int players, GameConfiguration configuration, IGameRepository repository)
    {
        List<MenuItem> menuItems = new();
        var playerNames = new Dictionary<ChoiceMenuItem, TextMenuItem>();

        for (var i = 1; i <= players; i++)
        {
            var playerType = new ChoiceMenuItem($"Player {i} type: ")
            {
                Choices = new[] {"Human", "Computer"}
            };
            var playerName = new TextMenuItem($"Player {i} name: ")
            {
                MaxLength = 32,
                RegexPattern = @"^\w[\w_.+\-@ ]*\w$",
                CharValidationFunc = ValidNameChar,
                ValidationErrorMessage = "Player name must be at least 2 characters long, start and end with a letter, digit or an underscore."
            };

            playerNames.Add(playerType, playerName);

            menuItems.Add(playerType);
            menuItems.Add(playerName);
            menuItems.Add(new MenuItem("") { Selectable = false });
        }

        menuItems.Add(new MenuItem("Start game")
        {
            Type = EMenuItemType.Continue,
            ValidationFunc = () => AreNamesUnique(playerNames.Values),
            ValidationErrorMessage = "Player names must be unique.",
            Action = () => GameLoop.StartNewGame(gameName, CreatePlayers(playerNames), configuration, repository)
        });

        return new MenuLevel("Players", menuItems);
    }

    private static bool ValidNameChar(char c) => char.IsLetterOrDigit(c) || "_.+-@ ".Contains(c);

    private static bool AreNamesUnique(IEnumerable<TextMenuItem> names)
    {
        var playerNames = names
            .Select(item => item.TextValue)
            .ToList();
        return playerNames.Count == playerNames.Distinct().Count();
    }

    private static List<Player> CreatePlayers(Dictionary<ChoiceMenuItem, TextMenuItem> configs)
    {
        var players = new List<Player>();
        foreach (var (playerType, playerName) in configs)
        {
            Player player = playerType.GetCurrent() switch
            {
                "Human" => new Player(playerName.TextValue, EPlayerType.Human),
                "Computer" => new Player(playerName.TextValue, EPlayerType.Computer),
                { } s => throw new ArgumentOutOfRangeException(paramName: s)
            };
            players.Add(player);
        }

        return players;
    }
}