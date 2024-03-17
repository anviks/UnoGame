namespace MenuSystem.MenuItems;

public enum EMenuItemType
{
    Default,  // MenuLevel does not change upon choosing a Default-type MenuItem
    Continue,  // All MenuItems need to be valid before choosing a Continue-type MenuItem
    Exit,  // MenuItems do not need to be valid upon choosing an Exit-type MenuItem
}