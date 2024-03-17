namespace MenuSystem.MenuItems;

public class ChoiceMenuItem: MenuItem
{
    public IEnumerable<string> Choices { get; set; } = default!;
    public int SelectedChoiceIndex { get; set; }
    public Action? OnChoiceChange { get; set; }

    public ChoiceMenuItem(string label) : base(label)
    {
    }

    public string GetCurrent()
    {
        return Choices.ElementAt(SelectedChoiceIndex);
    }

    public void Next()
    {
        SelectedChoiceIndex = (SelectedChoiceIndex + 1) % Choices.Count();
    }

    public void Previous()
    {
        SelectedChoiceIndex = (SelectedChoiceIndex - 1 + Choices.Count()) % Choices.Count();
    }

    public override void Unload()
    {
        SelectedChoiceIndex = 0;
        base.Unload();
    }
}