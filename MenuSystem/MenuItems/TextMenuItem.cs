using System.Text.RegularExpressions;

namespace MenuSystem.MenuItems;

public class TextMenuItem : MenuItem
{
    public string TextValue { get; set; } = "";
    public int MaxLength { get; set; }
    public string RegexPattern { get; set; } = ".*";
    public Predicate<char> CharValidationFunc { get; set; } = char.IsLetter;


    public TextMenuItem(string label) : base(label)
    {
    }

    public void AddChar(char letter)
    {
        if (TextValue.Length < MaxLength && CharValidationFunc(letter))
        {
            TextValue += letter;
        }
    }

    public void DeleteChar()
    {
        if (TextValue.Length > 0) TextValue = TextValue.Remove(TextValue.Length - 1);
    }

    public override bool IsValidState()
    {
        return Regex.IsMatch(TextValue, RegexPattern);
    }

    public override void Unload()
    {
        TextValue = "";
        base.Unload();
    }
}