using MiaCrate.Client.Utils;
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

    public new bool MouseClicked(double x, double y, MouseButton button)
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
    bool IGuiEventListener.MouseClicked(double x, double y, MouseButton button) => MouseClicked(x, y, button);

    public new bool MouseDragged(double x, double y, MouseButton button, double x2, double y2) => 
        FocusedChild != null && IsDragging && button == 0 && FocusedChild.MouseDragged(x, y, button, x2, y2);
    bool IGuiEventListener.MouseDragged(double x, double y, MouseButton button, double x2, double y2) =>
        MouseDragged(x, y, button, x2, y2);

    public new bool MouseScrolled(double x, double y, double amount) =>
        GetChildAt(x, y).Where(n => n.MouseScrolled(x, y, amount)).IsPresent;
    bool IGuiEventListener.MouseScrolled(double x, double y, double amount) => MouseScrolled(x, y, amount);

    public new bool KeyPressed(Keys key, int scancode, KeyModifiers modifiers) => 
        FocusedChild != null && FocusedChild.KeyPressed(key, scancode, modifiers);

    bool IGuiEventListener.KeyPressed(Keys key, int scancode, KeyModifiers modifiers) => KeyPressed(key, scancode, modifiers);

    public new bool KeyReleased(Keys key, int scancode, KeyModifiers modifiers) =>
        FocusedChild != null && FocusedChild.KeyReleased(key, scancode, modifiers);
    bool IGuiEventListener.KeyReleased(Keys key, int scancode, KeyModifiers modifiers) => KeyReleased(key, scancode, modifiers);
}