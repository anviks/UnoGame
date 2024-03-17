// #define TEST

using ConsoleApp;
using DAL;
using Domain;
using Domain.Cards;
using MenuSystem;
using MenuSystem.MenuItems;
using UnoEngine;

#if TEST

Console.WriteLine("heloo");

string input = "PascalCaseExampleABSC";

string pattern = "(?<!^)([A-Z])";
string replacement = "-$1";
string output = Regex.Replace(input, pattern, replacement, RegexOptions.Compiled)
    .ToLower();

Console.WriteLine(output);

#else


// ############################
// #  Configuration handling  #
// ############################


var gameConfig = new GameConfiguration();

var saveFileOrderOptions = Enum.GetNames(typeof(EGameOrder));
var ascDesc = new[] { "Ascending", "Descending" };


// ###################
// #  New game menu  #
// ###################


var storage = GameStorageJson.Instance;
// var storage = GameStorageDb.Instance;

var menu = new Menu(ConsoleColor.Green);
var blankItem = new MenuItem("") { Selectable = false };

var gameNameItem = new TextMenuItem("Game name: ")
{
    MaxLength = 32,
    RegexPattern = @"^\w[\w_.+\-@ ]*\w$",
    CharValidationFunc = c => char.IsLetterOrDigit(c) || "_.+-@ ".Contains(c),
    ValidationErrorMessage = "Game name must be at least 2 characters long, start and end with a letter, digit or an underscore."
};

var playerCountItem = new NumericMenuItem("Players: ", 2, 10)
{
    ManualInput = true
};

var removeCardColorItem = new ChoiceMenuItem("Remove card color: ")
{
    Choices = Enum.GetNames(typeof(ECardColor)).Prepend("All").Prepend("None")
};

var removeCardValueItem = new ChoiceMenuItem("Remove card value: ")
{
    Choices = Enum.GetNames(typeof(ECardValue)).Prepend("All").Prepend("None")
};

var removeButton = new MenuItem("Remove")
{
    Type = EMenuItemType.Default,
    Action = () =>
    {
        var color = removeCardColorItem.GetCurrent();
        var value = removeCardValueItem.GetCurrent();

        foreach (var card in CardDeck.DefaultCards)
        {
            if (color == "None" || value == "None") continue;

            if ((color == card.Color.ToString() || color == "All")
                && (value == card.Value.ToString() || value == "All"))
            {
                gameConfig.DisabledCards.Add(card);
            }
        }
    },
    UnloadAction = gameConfig.DisabledCards.Clear
};

var newGame = new MenuLevel("New game")
{
    MenuItems = new List<MenuItem>
    {
        gameNameItem,
        playerCountItem,
        blankItem,
        removeCardColorItem,
        removeCardValueItem,
        removeButton,
        blankItem,
        new("Continue")
        {
            Type = EMenuItemType.Continue,
            Action = () =>
                menu.EnterMenu(
                    UnoMenuCreator.CreatePlayerMenuLevel(
                        gameNameItem.TextValue, playerCountItem.CurrentValue, gameConfig, storage),
                    addBlank: false),
            ValidationFunc = () => Engine.IsValidConfiguration(gameConfig, playerCountItem.CurrentValue),
            ValidationErrorMessage = "Not enough cards to start the game."
        }
    }
};

var mainMenuItems = new List<MenuItem>
{
    new("New Game") { Action = () => menu.EnterMenu(newGame, false) }
};


// ####################
// #  Game save menu  #
// ####################


var savedGames = storage.FetchAllGames().OrderBy(entity => entity.CreatedAt).ToList();

if (savedGames.Any())
{
    var gameSaveMenu = new MenuLevel("Saves", menuItems: new List<MenuItem>());
    var gameDeleteMenu = new MenuLevel("Delete game", menuItems: new List<MenuItem>());

    var orderChoiceItem = new ChoiceMenuItem("Order by: ") { Choices = saveFileOrderOptions };
    var orderChoiceItemAscDesc = new ChoiceMenuItem("Order by: ") { Choices = ascDesc };

    var currentOrderBy = StringToOrder(orderChoiceItem.GetCurrent());
    var currentAscDesc = orderChoiceItemAscDesc.GetCurrent();

    var idToMenuItem = new Dictionary<Guid, MenuItem>();

    MenuItem GameToContinueMenuItem(GameState state)
    {
        var continueMenuItem = new MenuItem(state.GameName)
        {
            Type = EMenuItemType.Continue,
            Action = () => GameLoop.ContinueGame(storage, state.Id)
        };
        idToMenuItem[state.Id] = continueMenuItem;

        return continueMenuItem;
    }

    MenuItem GameToDeleteMenuItem(GameState state)
    {
        var deleteMenuItem = new MenuItem(state.GameName)
        {
            Type = EMenuItemType.Default
        };

        deleteMenuItem.Action = () =>
        {
            gameSaveMenu.MenuItems.RemoveAll(menuItem => idToMenuItem.ContainsKey(state.Id) && menuItem == idToMenuItem[state.Id]);
            gameDeleteMenu.MenuItems.Remove(deleteMenuItem);
            storage.DeleteGame(state.Id);
            savedGames.Remove(state);
            if (savedGames.Any()) return;
            mainMenuItems.RemoveAt(1);
            mainMenuItems.RemoveAt(1);
        };

        return deleteMenuItem;
    }

    EGameOrder StringToOrder(string u)
    {
        Console.WriteLine(u);
        Enum.TryParse(typeof(EGameOrder), u, out var enumObj);
        if (enumObj == null) throw new NullReferenceException("aaaaaaaaaaa");
        return (EGameOrder)enumObj;
    }

    gameSaveMenu.MenuItems.Add(orderChoiceItem);
    gameSaveMenu.MenuItems.Add(orderChoiceItemAscDesc);
    gameSaveMenu.MenuItems.Add(blankItem);
    gameSaveMenu.MenuItems.AddRange(savedGames.Select(GameToContinueMenuItem));

    gameDeleteMenu.MenuItems.Add(orderChoiceItem);
    gameDeleteMenu.MenuItems.Add(orderChoiceItemAscDesc);
    gameDeleteMenu.MenuItems.Add(blankItem);
    gameDeleteMenu.MenuItems.AddRange(savedGames.Select(GameToDeleteMenuItem));

    void UpdateOrder(ChoiceMenuItem item, MenuLevel menuLevel, Func<GameState, MenuItem> gameToMenuItem)
    {
        if (menuLevel.MenuItems.Count < 3 + savedGames.Count) return;
        menuLevel.MenuItems.RemoveRange(3, savedGames.Count);
        if (item == orderChoiceItem)
            currentOrderBy = StringToOrder(item.GetCurrent());
        else
            currentAscDesc = item.GetCurrent();
        var savedGamesCopy = OrderGames(savedGames, currentOrderBy, currentAscDesc == "Ascending");
        menuLevel.MenuItems.InsertRange(3, savedGamesCopy.Select(gameToMenuItem));
    }

    orderChoiceItem.OnChoiceChange = () =>
    {
        UpdateOrder(orderChoiceItem, gameSaveMenu, GameToContinueMenuItem);
        UpdateOrder(orderChoiceItem, gameDeleteMenu, GameToDeleteMenuItem);
    };
    orderChoiceItemAscDesc.OnChoiceChange = () =>
    {
        UpdateOrder(orderChoiceItemAscDesc, gameSaveMenu, GameToContinueMenuItem);
        UpdateOrder(orderChoiceItemAscDesc, gameDeleteMenu, GameToDeleteMenuItem);
    };

    IEnumerable<GameState> OrderGames(List<GameState> games, EGameOrder orderBy, bool ascending)
    {
        games = games.OrderBy<GameState, IComparable>(orderBy switch
        {
            EGameOrder.CreationTime => entity => entity.CreatedAt,
            EGameOrder.LastWriteTime => entity => entity.UpdatedAt,
            EGameOrder.Name => entity => entity.GameName,
            _ => entity => entity.CreatedAt
        }).ToList();

        if (!ascending) games.Reverse();

        return games;
    }

    mainMenuItems.Insert(
        1,
        new MenuItem("Delete Game")
        {
            Type = EMenuItemType.Continue,
            Action = () =>
            {
                UpdateOrder(orderChoiceItem, gameDeleteMenu, GameToDeleteMenuItem);
                UpdateOrder(orderChoiceItemAscDesc, gameDeleteMenu, GameToDeleteMenuItem);
                menu.EnterMenu(gameDeleteMenu);
            }
        }
    );

    mainMenuItems.Insert(
        1,
        new MenuItem("Continue Game")
        {
            Type = EMenuItemType.Continue,
            Action = () =>
            {
                UpdateOrder(orderChoiceItem, gameSaveMenu, GameToContinueMenuItem);
                UpdateOrder(orderChoiceItemAscDesc, gameSaveMenu, GameToContinueMenuItem);
                menu.EnterMenu(gameSaveMenu);
            }
        }
    );
}

var mainMenu = new MenuLevel("UNO")
{
    MenuItems = mainMenuItems
};

menu.EnterMenu(mainMenu);

#endif