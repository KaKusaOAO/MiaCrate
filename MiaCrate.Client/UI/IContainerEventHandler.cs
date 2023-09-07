using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public interface IContainerEventHandler : IGuiEventListener
{
    public List<IGuiEventListener> Children { get; }

    public IOptional<IGuiEventListener> GetChildAt(double x, double y)
    {
        foreach (var child in Children)
        {
            if (child.IsMouseOver(x, y)) return Optional.Of(child);
        }
        
        return Optional.Empty<IGuiEventListener>();
    }

    public IGuiEventListener? FocusedChild { get; set; }
    
    public bool IsDragging { get; set; }

    public new bool IsFocused => FocusedChild != null;

    bool IGuiEventListener.IsFocused
    {
        get => IsFocused; 
        set {}
    }

    public new bool MouseClicked(double x, double y, int button)
    {
        foreach (var child in Children)
        {
            if (!child.MouseClicked(x, y, button)) continue;
            FocusedChild = child;
            if (button == 0) IsDragging = true;
            return true;
        }

        return false;
    }

    bool IGuiEventListener.MouseClicked(double x, double y, int button) => MouseClicked(x, y, button);

    public new bool MouseDragged(double x, double y, int button, double x2, double y2) => 
        FocusedChild != null && IsDragging && button == 0 && FocusedChild.MouseDragged(x, y, button, x2, y2);

    bool IGuiEventListener.MouseDragged(double x, double y, int button, double x2, double y2) =>
        MouseDragged(x, y, button, x2, y2);

    public new bool MouseScrolled(double x, double y, double amount) =>
        GetChildAt(x, y).Where(n => n.MouseScrolled(x, y, amount)).IsPresent;

    bool IGuiEventListener.MouseScrolled(double x, double y, double amount) => MouseScrolled(x, y, amount);

    public new bool KeyPressed(int i, int j, int k) => 
        FocusedChild != null && FocusedChild.KeyPressed(i, j, k);

    bool IGuiEventListener.KeyPressed(int i, int j, int k) => KeyPressed(i, j, k);

    public new bool KeyReleased(int i, int j, int k) =>
        FocusedChild != null && FocusedChild.KeyReleased(i, j, k);

    bool IGuiEventListener.KeyReleased(int i, int j, int k) => KeyReleased(i, j, k);
}