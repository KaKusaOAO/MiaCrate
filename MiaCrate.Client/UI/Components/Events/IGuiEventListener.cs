using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.UI;

public interface IGuiEventListener
{
    public void MouseMoved(double x, double y) {}
    public bool MouseClicked(double x, double y, MouseButton button) => false;
    public bool MouseReleased(double x, double y, int button) => false;
    public bool MouseDragged(double x, double y, MouseButton button, double x2, double y2) => false;
    public bool MouseScrolled(double x, double y, double amount) => false;
    public bool KeyPressed(Keys key, int scancode, KeyModifiers modifiers) => false;
    public bool KeyReleased(Keys key, int scancode, KeyModifiers modifiers) => false;
    public bool CharTyped(char c, KeyModifiers modifiers) => false;
    public bool IsMouseOver(double x, double y) => false;
    public bool IsFocused { get; set; }
}