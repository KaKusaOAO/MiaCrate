using MiaCrate.Client.Sodium.UI;
using MiaCrate.Client.Utils;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.UI.Screens;

public class OptionsScreen : Screen
{
    private static IComponent SkinCustomization { get; } = MiaComponent.Translatable("options.skinCustomisation");
    private static IComponent Sounds { get; } = MiaComponent.Translatable("options.sounds");
    private static IComponent Video { get; } = MiaComponent.Translatable("options.video");
    private static IComponent Controls { get; } = MiaComponent.Translatable("options.controls");
    private static IComponent Language { get; } = MiaComponent.Translatable("options.language");
    private static IComponent Chat { get; } = MiaComponent.Translatable("options.chat");
    private static IComponent ResourcePack { get; } = MiaComponent.Translatable("options.resourcepack");
    private static IComponent Accessibility { get; } = MiaComponent.Translatable("options.accessibility");
    private static IComponent Telemetry { get; } = MiaComponent.Translatable("options.telemetry");
    private static IComponent CreditsAndAttribution { get; } = MiaComponent.Translatable("options.credits_and_attribution");

    private const int Columns = 2;
    
    private readonly Screen _lastScreen;
    private readonly Options _options;

    public OptionsScreen(Screen lastScreen, Options options) 
        : base(MiaComponent.Translatable("options.title"))
    {
        _lastScreen = lastScreen;
        _options = options;
    }

    protected override void Init()
    {
        var layout = new GridLayout();
        layout.DefaultCellSetting.SetPaddingHorizontal(5).SetPaddingBottom(4).AlignHorizontallyCenter();

        var rowHelper = layout.CreateRowHelper(Columns);

        rowHelper.AddChild(_options.Fov.CreateButton(Game!.Options, 0, 0, 150));
        
        rowHelper.AddChild(SpacerElement.CreateHeight(Button.DefaultHeight + 6), Columns);
        
        rowHelper.AddChild(OpenScreenButton(SkinCustomization, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Sounds, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Video,
            () => SharedConstants.IncludesSodium ? new SodiumOptionsScreen(this) : _lastScreen
            ));
        rowHelper.AddChild(OpenScreenButton(Controls, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Language, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Chat, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(ResourcePack, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Accessibility, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(Telemetry, () => _lastScreen));
        rowHelper.AddChild(OpenScreenButton(CreditsAndAttribution, () => _lastScreen));

        rowHelper.AddChild(Button.CreateBuilder(CommonComponents.GuiDone, _ => Game.Screen = _lastScreen)
            .Width(200).Build(), Columns, rowHelper.NewCellSettings().SetPaddingTop(6));
        
        layout.ArrangeElements();
        FrameLayout.AlignInRectangle(layout, 0, Height / 6 - 12, Width, Height, 0.5f, 0);
        layout.VisitWidgets(w => AddRenderableWidget(w));
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        base.Render(graphics, mouseX, mouseY, f);
        graphics.DrawCenteredString(Font, Title, Width / 2, 15, Argb32.White);
    }

    private Button OpenScreenButton(IComponent component, Func<Screen> screen) => 
        Button.CreateBuilder(component, _ => Game!.Screen = screen()).Build();
}