using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.UI;

public interface IGuiEventListener
{
    public void MouseMoved(double x, double y) {}
    public bool MouseClicked(double x, double y, MouseButton button) => false;
    public bool MouseReleased(double x, double y, MouseButton button) => false;
    public bool MouseDragged(double x, double y, MouseButton button, double dx, double dy) => false;
    public bool MouseScrolled(double x, double y, double amount) => false;
    public bool KeyPressed(Keys key, int scancode, KeyModifiers modifiers) => false;
    public bool KeyReleased(Keys key, int scancode, KeyModifiers modifiers) => false;
    public bool CharTyped(char c, KeyModifiers modifiers) => false;
    public bool IsMouseOver(double x, double y) => false;
    public bool IsFocused { get; set; }
}

public static class GuiEventListenerExtension
{
    public static void MouseMoved(this IGuiEventListener listener, double x, double y) =>
        listener.MouseMoved(x, y);

    public static bool MouseClicked(this IGuiEventListener listener, double x, double y, MouseButton button) =>
        listener.MouseClicked(x, y, button);
    
    public static bool MouseReleased(this IGuiEventListener listener, double x, double y, MouseButton button) =>
        listener.MouseReleased(x, y, button);

    public static bool MouseDragged(this IGuiEventListener listener, double x, double y, MouseButton button, double dx,
        double dy) =>
        listener.MouseDragged(x, y, button, dx, dy);
    
    public static bool MouseScrolled(this IGuiEventListener listener, double x, double y, double amount) =>
        listener.MouseScrolled(x, y, amount);

    public static bool KeyPressed(this IGuiEventListener listener, Keys key, int scancode, KeyModifiers modifiers) =>
        listener.KeyPressed(key, scancode, modifiers);
    
    public static bool KeyReleased(this IGuiEventListener listener, Keys key, int scancode, KeyModifiers modifiers) => 
        listener.KeyReleased(key, scancode, modifiers);
    public static bool CharTyped(this IGuiEventListener listener, char c, KeyModifiers modifiers) => 
        listener.CharTyped(c, modifiers);

    public static bool IsMouseOver(this IGuiEventListener listener, double x, double y) =>
        listener.IsMouseOver(x, y);
}