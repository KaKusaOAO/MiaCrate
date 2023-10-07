using MiaCrate.Client.UI;
using MiaCrate.Client.Utils;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options.Controls;

public interface IControlElement
{
    public AbstractSodiumWidget AsWidget() => (AbstractSodiumWidget) this;
}

public class ControlElement<T> : AbstractSodiumWidget, IControlElement
{
    protected ISodiumOption<T> Option { get; }
    protected Dim2I Dimension { get; }
    
    public ControlElement(ISodiumOption<T> option, Dim2I dimension)
    {
        Option = option;
        Dimension = dimension;
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var name = Option.Name.ToPlainText();
        string label;

        if ((_hovered || IsFocused) && Font.Width(name) > Dimension.Width - Option.Control.MaxWidth)
        {
            name = name[..Math.Min(name.Length, 10)] + "...";
        }

        if (Option.IsAvailable)
        {
            if (Option.HasChanged)
            {
                label = ChatFormatting.Italic + name + " *";
            }
            else
            {
                label = ChatFormatting.White + name;
            }
        }
        else
        {
            label = "" + ChatFormatting.Gray + ChatFormatting.Strikethrough + name;
        }

        _hovered = Dimension.ContainsCursor(mouseX, mouseY);
        
        DrawRect(graphics, Dimension.X, Dimension.Y, Dimension.LimitX, Dimension.LimitY, _hovered ? 0xe0000000 : 0x90000000);
        DrawString(graphics, label, Dimension.X + 6, Dimension.CenterY - 4, Argb32.White);

        if (IsFocused)
        {
            DrawBorder(graphics, Dimension.X, Dimension.Y, Dimension.LimitX, Dimension.LimitY, -1);
        }
    }
}