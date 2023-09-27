using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.UI;

public abstract class AbstractContainerEventHandler : IContainerEventHandler
{
    private IGuiEventListener? _focusedChild;
    public abstract List<IGuiEventListener> Children { get; }

    public virtual IGuiEventListener? FocusedChild
    {
        get => _focusedChild;
        set
        {
            if (_focusedChild != null) _focusedChild.IsFocused = false;
            if (value != null) value.IsFocused = true;
            _focusedChild = value;
        }
    }

    public bool IsDragging { get; set; }
}