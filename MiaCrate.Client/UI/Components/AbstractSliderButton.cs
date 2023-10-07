using MiaCrate.Client.Systems;
using MiaCrate.Client.UI.Narration;
using MiaCrate.Client.Utils;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public abstract class AbstractSliderButton : AbstractWidget
{
    private static ResourceLocation SliderSprite { get; } = new("widget/slider");
    private static ResourceLocation HighlightedSprite { get; } = new("widget/slider_highlighted");
    private static ResourceLocation SliderHandleSprite { get; } = new("widget/slider_handle");
    private static ResourceLocation SliderHandleHighlightedSprite { get; } = new("widget/slider_handle_highlighted");

    protected const int TextMargin = 2;
    private const int HandleWidth = 8;
    private const int HandleHalfWidth = HandleWidth / 2;

    private bool _canChangeValue;
    
    protected double Value { get; set; }

    protected AbstractSliderButton(int x, int y, int width, int height, IComponent message, double value)
        : base(x, y, width, height, message)
    {
        Value = value;
    }

    private ResourceLocation GetSprite() => IsFocused && !_canChangeValue ? HighlightedSprite : SliderSprite;

    private ResourceLocation GetHandleSprite() =>
        !IsHovered && !_canChangeValue ? SliderHandleSprite : SliderHandleHighlightedSprite;

    protected override IMutableComponent CreateNarrationMessage() => 
        MiaComponent.Translatable("gui.narrate.slider", Message);

    protected override void UpdateWidgetNarration(INarrationElementOutput output)
    {
        output.Add(NarratedElementType.Title, CreateNarrationMessage());

        if (IsActive)
        {
            if (IsFocused)
            {
                output.Add(NarratedElementType.Usage, MiaComponent.Translatable("narration.slider.usage.focused"));
            }
            else
            {
                output.Add(NarratedElementType.Usage, MiaComponent.Translatable("narration.slider.usage.hovered"));
            }
        }
    }

    protected override void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var game = Game.Instance;
        graphics.SetColor(1, 1, 1, Alpha);
        
        RenderSystem.EnableBlend();
        RenderSystem.DefaultBlendFunc();
        RenderSystem.EnableDepthTest();
        
        graphics.BlitSprite(GetSprite(), X, Y, Width, Height);
        graphics.BlitSprite(GetHandleSprite(), X + (int) (Value * (Width - HandleWidth)), Y, HandleWidth, Height);
        graphics.SetColor(1, 1, 1, 1);

        var k = (Argb32) (IsActive ? 0xFFFFFF : 0xA0A0A0);
        RenderScrollingString(graphics, game.Font, TextMargin, k.WithAlpha(Alpha));
    }

    public override void OnClick(double x, double y) => SetValueFromMouse(x);

    protected override void OnDrag(double x, double y, double dx, double dy)
    {
        SetValueFromMouse(x);
        base.OnDrag(x, y, dx, dy);
    }

    private void SetValueFromMouse(double x) => SetValue((x - (X + HandleHalfWidth)) / (Width - HandleWidth));

    private void SetValue(double value)
    {
        var e = Value;
        Value = Math.Clamp(value, 0, 1);
        
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (e != Value)
        {
            ApplyValue();
        }

        UpdateMessage();
    }

    protected abstract void ApplyValue();
    protected abstract void UpdateMessage();
}