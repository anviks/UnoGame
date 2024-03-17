using MenuSystem.MenuItems;

namespace MenuSystem;

public class Menu
{
    private Stack<MenuLevel> Levels { get; } = new();

    private readonly Dictionary<string, MenuItem> _reservedItems = new();
    private ConsoleColor SelectorColor { get; set; }

    public Menu(ConsoleColor selectorColor)
    {
        SelectorColor = selectorColor;
        Console.CursorVisible = false;

        _reservedItems.Add("Back", new MenuItem("Back") { Action = Back, Type = EMenuItemType.Exit });
        _reservedItems.Add("Main", new MenuItem("Main Menu") { Action = ReturnToMain, Type = EMenuItemType.Exit });
        _reservedItems.Add("Exit", new MenuItem("Exit") { Action = () => Environment.Exit(0), Type = EMenuItemType.Exit });
        _reservedItems.Add("Blank", new MenuItem("") { Selectable = false });
    }

    public void EnterMenu(MenuLevel level, bool addBlank = true)
    {
        if (addBlank) level.MenuItems.Add(_reservedItems["Blank"]);
        if (Levels.Count > 0) level.MenuItems.Add(_reservedItems["Back"]);
        if (Levels.Count > 1) level.MenuItems.Add(_reservedItems["Main"]);
        level.MenuItems.Add(_reservedItems["Exit"]);
        level.UpdateWidth();
        level.SelectorColor = SelectorColor;
        Levels.Push(level);
        RunCurrentLevel();
    }

    private void Back()
    {
        UnloadCurrentLevel();
        RunCurrentLevel();
    }

    private void ReturnToMain()
    {
        while (Levels.Count > 1)
        {
            UnloadCurrentLevel();
        }

        RunCurrentLevel();
    }

    private void UnloadCurrentLevel()
    {
        var level = Levels.Pop();
        level.Unload();
        level.MenuItems.RemoveAll(_reservedItems.ContainsValue);
    }

    private void RunCurrentLevel()
    {
        Levels.Peek().Run()();
    }
}