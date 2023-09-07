namespace MiaCrate.Client;

public enum InputType
{
    None,
    Mouse,
    KeyboardArrow,
    KeyboardTab
}

public static class InputTypeExtension
{
    public static bool IsMouse(this InputType type) => type == InputType.Mouse;
    public static bool IsKeyboard(this InputType type) => type is InputType.KeyboardArrow or InputType.KeyboardTab;
}