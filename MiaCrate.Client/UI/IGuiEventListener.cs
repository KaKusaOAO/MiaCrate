namespace MiaCrate.Client.UI;

public interface IGuiEventListener
{
    public void MouseMoved(double x, double y) {}
    public bool MouseClicked(double x, double y, int button) => false;
    public bool MouseReleased(double x, double y, int button) => false;
    public bool MouseDragged(double x, double y, int button, double x2, double y2) => false;
    public bool MouseScrolled(double x, double y, double amount) => false;
    public bool KeyPressed(int i, int j, int k) => false;
    public bool KeyReleased(int i, int j, int k) => false;
    public bool CharTyped(char c, int i) => false;
    public bool IsMouseOver(double x, double y) => false;
    public bool IsFocused { get; set; }
}