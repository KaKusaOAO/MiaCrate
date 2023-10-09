using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using MiaCrate.Client.Utils;
using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.IrisShader.UI;

public class ShaderPackScreen : Screen
{
    private readonly Screen _parent;

    private ShaderPackSelectionList _shaderPackList;

    public ShaderPackScreen(Screen parent) 
        : base(MiaComponent.Translatable("options.iris.shaderPackSelection.title"))
    {
        _parent = parent;
    }

    protected override void Init()
    {
        base.Init();
        
        RemoveWidget(_shaderPackList);

        _shaderPackList = new ShaderPackSelectionList(this, Game!, Width, Height, 32, Height - 58, 0, Width);
        
        ClearWidgets();

        AddRenderableWidget(_shaderPackList);

        var bottomCenter = Width / 2 - 50;
        var topCenter = Width / 2 - 76;

        AddRenderableWidget(Button.CreateBuilder(CommonComponents.GuiDone, _ => Game!.Screen = _parent)
            .Bounds(bottomCenter + 104, Height - 27, 100, 20)
            .Build());
        
        AddRenderableWidget(Button.CreateBuilder(MiaComponent.Translatable("options.iris.apply"), _ => Game!.Screen = _parent)
            .Bounds(bottomCenter, Height - 27, 100, Button.DefaultHeight)
            .Build());
            
        AddRenderableWidget(Button.CreateBuilder(CommonComponents.GuiCancel, _ => Game!.Screen = _parent)
            .Bounds(bottomCenter - 104, Height - 27, 100, Button.DefaultHeight)
            .Build());
        
        AddRenderableWidget(Button.CreateBuilder(MiaComponent.Translatable("options.iris.openShaderPackFolder"), _ => Game!.Screen = _parent)
            .Bounds(topCenter - 78, Height - 51, 152, Button.DefaultHeight)
            .Build());
        
        AddRenderableWidget(Button.CreateBuilder(MiaComponent.Translatable("options.iris.shaderPackList"), _ => Game!.Screen = _parent)
            .Bounds(topCenter + 78, Height - 51, 152, Button.DefaultHeight)
            .Build());
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        base.Render(graphics, mouseX, mouseY, f);
        
        graphics.DrawCenteredString(Font, Title, Width / 2, 8, Argb32.White);
        graphics.DrawCenteredString(Font, MiaComponent.Translatable("pack.iris.select.title").WithColor(TextColor.Gray).WithItalic(true), Width / 2, 21, Argb32.White);
    }
}