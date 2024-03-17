namespace MenuSystem.MenuItems;

public class NumericMenuItem : MenuItem
{
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public bool ManualInput { get; set; }
    public int CurrentValue { get; set; }

    public NumericMenuItem(string label, int minValue, int maxValue) : base(label)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = ClosestToZero();
    }

    public void AddDigit(ConsoleKeyInfo pressedKey)
    {
        try
        {
            CurrentValue = int.Parse(CurrentValue + pressedKey.KeyChar.ToString());
            if (CurrentValue < MinValue ||
                CurrentValue > MaxValue)
            {
                CurrentValue = FarthestFromZero();
            }
        }
        catch (OverflowException)
        {
            CurrentValue = FarthestFromZero();
        }
    }

    public void DeleteDigit()
    {
        var valueString = CurrentValue.ToString();
        if (valueString.Length == 1 || valueString is ['-', _])
        {
            CurrentValue = ClosestToZero();
        }
        else
        {
            RemoveLastDigit(valueString);
        }
    }

    private void RemoveLastDigit(string valueString)
    {
        CurrentValue = int.Parse(valueString.Remove(valueString.Length - 1));
        if (CurrentValue < MinValue || CurrentValue > MaxValue)
        {
            CurrentValue = ClosestToZero();
        }
    }

    public void InvertNumericItemValue()
    {
        CurrentValue = -CurrentValue;

        if (CurrentValue < MinValue) CurrentValue = MinValue;
        else if (CurrentValue > MaxValue) CurrentValue = MaxValue;
    }

    public int ClosestToZero()
    {
        if (MinValue < 0 && MaxValue > 0) return 0;
        return Math.Abs(MinValue) < Math.Abs(MaxValue) ? MinValue : MaxValue;
    }

    public int FarthestFromZero()
    {
        return Math.Abs(MinValue) > Math.Abs(MaxValue) ? MinValue : MaxValue;
    }

    public override void Unload()
    {
        CurrentValue = ClosestToZero();
        base.Unload();
    }
}