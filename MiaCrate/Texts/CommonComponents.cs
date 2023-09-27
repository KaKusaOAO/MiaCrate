using Mochi.Texts;

namespace MiaCrate.Texts;

public static class CommonComponents
{
    public static IComponent Empty { get; } = MiaComponent.Literal("");
    public static IComponent OptionOn { get; } = MiaComponent.Translatable("options.on");
    public static IComponent OptionOff { get; } = MiaComponent.Translatable("options.off");
    public static IComponent GuiDone { get; } = MiaComponent.Translatable("gui.done");
    public static IComponent GuiCancel { get; } = MiaComponent.Translatable("gui.cancel");
    public static IComponent GuiYes { get; } = MiaComponent.Translatable("gui.yes");
    public static IComponent GuiNo { get; } = MiaComponent.Translatable("gui.no");
    public static IComponent GuiOk { get; } = MiaComponent.Translatable("gui.ok");
    public static IComponent GuiProceed { get; } = MiaComponent.Translatable("gui.proceed");
    public static IComponent GuiContinue { get; } = MiaComponent.Translatable("gui.continue");
    public static IComponent GuiBack { get; } = MiaComponent.Translatable("gui.back");
    public static IComponent GuiToTitle { get; } = MiaComponent.Translatable("gui.toTitle");
    public static IComponent NarrationSeparator { get; } = MiaComponent.Literal(". ");
    public static IComponent Ellipsis { get; } = MiaComponent.Literal("...");
    public static IComponent Space { get; } = MiaComponent.Literal(" ");
}