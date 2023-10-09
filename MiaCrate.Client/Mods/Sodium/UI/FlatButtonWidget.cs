using MiaCrate.Client.Graphics;
using MiaCrate.Client.UI;
using MiaCrate.Client.Utils;
using Mochi.Texts;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Sodium.UI;

public class FlatButtonWidget : AbstractSodiumWidget
{
    private readonly Dim2I _dim;
    private readonly Action _action;

    private bool _selected;
    private bool _enabled = true;
    private bool _visible = true;

    public IComponent Label { get; set; }

    public FlatButtonWidget(Dim2I dim, IComponent label, Action action)
    {
        _dim = dim;
        Label = label;
        _action = action;
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        if (!_visible) return;

        _hovered = _dim.ContainsCursor(mouseX, mouseY);

        var backgroundColor = _enabled ? (_hovered ? 0xe0000000 : 0x90000000) : 0x60000000;
        var textColor = _enabled ? 0xffffffff : 0x90ffffff;
        var strWidth = Font.Width(Label);
        
        DrawRect(graphics, _dim.X, _dim.Y, _dim.LimitX, _dim.LimitY, backgroundColor);
        // DrawString(graphics, Label, _dim.CenterX - strWidth / 2, _dim.CenterY - 4, textColor);
        RenderScrollingString(graphics, Font, Label, _dim.X + 5, _dim.Y, _dim.LimitX - 5, _dim.LimitY, textColor);

        if (_enabled && _selected)
        {
            DrawRect(graphics, _dim.X, _dim.LimitY - 1, _dim.LimitX, _dim.LimitY, 0xff94e4d3);
        }

        if (_enabled && IsFocused)
        {
            DrawBorder(graphics, _dim.X, _dim.Y, _dim.LimitX, _dim.LimitY, -1);
        }
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
    }

    public override bool MouseClicked(double x, double y, MouseButton button)
    {
        if (!_enabled || !_visible) return false;
        if (button != MouseButton.Left || !_dim.ContainsCursor(x, y)) return false;
        
        DoAction();
        return true;
    }

    private void DoAction()
    {
        _action();
        PlayClickSound();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
    }
}