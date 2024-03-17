using Helpers;
using MenuSystem.MenuItems;

namespace MenuSystem;

public class MenuLevel
{
    public string Title { get; set; }
    public List<MenuItem> MenuItems { get; set; } = new();
    public int SelectedItemIndex { get; set; }
    private int Width { get; set; }
    public List<string> Errors { get; set; } = new();
    public ConsoleColor SelectorColor { get; set; }
    public const char RowSep = '=';
    public const char ColSep = '|';


    public MenuLevel(string title)
    {
        Title = title;
    }

    public MenuLevel(string title, IEnumerable<MenuItem> menuItems)
    {
        Title = title;
        MenuItems.AddRange(menuItems);
    }

    public void UpdateWidth()
    {
        var itemLengths = MenuItems.Select(
            item =>
            {
                var additionalLength = item switch
                {
                    NumericMenuItem numItem => numItem.CurrentValue.ToString().Length,
                    ChoiceMenuItem choiceItem => choiceItem.GetCurrent().Length,
                    TextMenuItem textItem => textItem.TextValue.Length,
                    _ => 0
                };
                return item.Label.Length + additionalLength;
            }).ToList();
        itemLengths.Add(Title.Length);
        Width = itemLengths.Max();
    }

    private static string Space(int count) => new(' ', count);

    private void DrawRowSeparator()
        => Console.WriteLine(new string(RowSep, Width + 8));

    private void DrawMenuItem(MenuItem item, bool isSelected)
    {
        Console.Write(ColSep);
        var leftSide = isSelected ? " > " : "   ";

        if (isSelected) Console.BackgroundColor = SelectorColor;

        var label = item.Label;
        Console.Write(leftSide + label);

        switch (item)
        {
            case NumericMenuItem numItem:
            {
                var currentValue = numItem.CurrentValue;
                Console.Write(currentValue + Space(Width - label.Length - currentValue.ToString().Length + 3));
                break;
            }
            case ChoiceMenuItem choiceItem:
            {
                var currentChoice = choiceItem.GetCurrent();
                Console.Write(currentChoice + Space(Width - label.Length - currentChoice.Length + 3));
                break;
            }
            case TextMenuItem textItem:
                var currentText = textItem.TextValue;
                Console.Write(currentText + Space(Width - label.Length - currentText.Length + 3));
                break;
            default:
                Console.Write(Space(Width - label.Length + 3));
                break;
        }

        Console.ResetColor();
        Console.WriteLine(ColSep);
    }

    private void Draw()
    {
        Console.Clear();
        DrawRowSeparator();
        Console.WriteLine(ColSep + "   " + Title + Space(Width - Title.Length + 3) + ColSep);
        DrawRowSeparator();
        for (var index = 0; index < MenuItems.Count; index++)
        {
            var item = MenuItems[index];
            var isSelected = index == SelectedItemIndex;
            DrawMenuItem(item, isSelected);
        }
        DrawRowSeparator();
        foreach (var error in Errors)
        {
            Console.WriteLine(AnsiConstants.RedBright + error);
        }
        Errors.Clear();
        Console.Write(AnsiConstants.Reset);
    }

    public Action Run()
    {
        Draw();

        while (true)
        {
            var pressedKey = Console.ReadKey(true);
            var selectedItem = MenuItems[SelectedItemIndex];

            switch (selectedItem)
            {
                case NumericMenuItem numericItem:
                    HandleNumericMenuItem(pressedKey, numericItem);
                    break;
                case ChoiceMenuItem choiceItem:
                    HandleChoiceMenuItem(pressedKey, choiceItem);
                    break;
                case TextMenuItem textItem:
                    HandleTextMenuItem(pressedKey, textItem);
                    break;
            }

            switch (pressedKey.Key)
            {
                case ConsoleKey.UpArrow:
                    DecrementSelectedIndex();
                    break;
                case ConsoleKey.DownArrow:
                    IncrementSelectedIndex();
                    break;
                case ConsoleKey.Enter:
                    // If the selected item is a Continue type, all items must be in valid state


                    if ((selectedItem.Type != EMenuItemType.Continue ||
                         MenuItems.TrueForAll(item => item.IsValidState()))
                        && selectedItem.Action != null)
                    {
                        if (selectedItem.Type == EMenuItemType.Default) selectedItem.Action();
                        else return selectedItem.Action;
                    }
                    else if (!MenuItems.TrueForAll(item => item.IsValidState())
                             && selectedItem.Type == EMenuItemType.Continue)
                    {
                        MenuItems.Where(item => !item.IsValidState())
                            .ToList()
                            .ForEach(item => Errors.Add(item.ValidationErrorMessage ?? "Some menu items are not in valid state."));
                    }
                    break;
                default:
                    continue;
            }

            Draw();
        }
    }

    private void IncrementSelectedIndex()
    {
        SelectedItemIndex = (SelectedItemIndex + 1) % MenuItems.Count;
        if (!MenuItems[SelectedItemIndex].Selectable) IncrementSelectedIndex(); // Skip non-selectable items
    }

    private void DecrementSelectedIndex()
    {
        SelectedItemIndex = (SelectedItemIndex - 1 + MenuItems.Count) % MenuItems.Count;
        if (!MenuItems[SelectedItemIndex].Selectable) DecrementSelectedIndex(); // Skip non-selectable items
    }

    /// <summary>
    /// Handles numeric menu item interactions based on user input.
    /// </summary>
    /// <param name="pressedKey">The key pressed by the user</param>
    /// <param name="numericItem">The numeric menu item being interacted with.</param>
    private void HandleNumericMenuItem(ConsoleKeyInfo pressedKey, NumericMenuItem numericItem)
    {
        // Determine the value change based on arrow key input
        var changeValue = pressedKey.Key switch
        {
            ConsoleKey.LeftArrow when numericItem.MinValue < numericItem.CurrentValue => -1,
            ConsoleKey.RightArrow when numericItem.MaxValue > numericItem.CurrentValue => 1,
            _ => 0
        };

        if (changeValue != 0)
        {
            numericItem.CurrentValue += changeValue;
            UpdateWidth();
            Draw();
        }

        if (!numericItem.ManualInput) return;

        if (char.IsDigit(pressedKey.KeyChar)) numericItem.AddDigit(pressedKey);
        else if (pressedKey.Key == ConsoleKey.Backspace) numericItem.DeleteDigit();
        else if (pressedKey.KeyChar == '-') numericItem.InvertNumericItemValue();
        else return;

        UpdateWidth();
        Draw();
    }

    private void HandleChoiceMenuItem(ConsoleKeyInfo pressedKey, ChoiceMenuItem choiceItem)
    {
        switch (pressedKey.Key)
        {
            case ConsoleKey.LeftArrow:
                choiceItem.Previous();
                choiceItem.OnChoiceChange?.Invoke();
                break;
            case ConsoleKey.RightArrow:
                choiceItem.Next();
                choiceItem.OnChoiceChange?.Invoke();
                break;
            default:
                return;
        }

        UpdateWidth();
        Draw();
    }

    private void HandleTextMenuItem(ConsoleKeyInfo pressedKey, TextMenuItem textItem)
    {
        if (pressedKey.Key == ConsoleKey.Backspace) textItem.DeleteChar();
        else if (textItem.CharValidationFunc(pressedKey.KeyChar)) textItem.AddChar(pressedKey.KeyChar);

        UpdateWidth();
        Draw();
    }

    public void Unload()
    {
        SelectedItemIndex = 0;
        MenuItems.ForEach(item => item.Unload());
    }
}