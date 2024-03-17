using System.Text.Json;

namespace MenuSystem.MenuItems;

public class MenuItem
{
    public string Label { get; }
    public Action? Action { get; set; }
    public bool Selectable { get; set; } = true;
    public Func<bool>? ValidationFunc { get; set; }
    public string? ValidationErrorMessage { get; set; }
    public Action? UnloadAction { get; set; }
    public EMenuItemType Type { get; set; } = EMenuItemType.Default;


    public MenuItem(string label)
    {
        Label = label;
    }

    public virtual bool IsValidState()
    {
        return ValidationFunc?.Invoke() ?? true;
    }

    public virtual void Unload()
    {
        UnloadAction?.Invoke();
    }
}