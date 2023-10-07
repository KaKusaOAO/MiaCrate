using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options.Controls;

public static class ControlValueFormatters
{
    public static ControlValueFormatter GuiScale { get; } = Create(v =>
    {
        return v == 0
            ? MiaComponent.Translatable("options.guiScale.auto")
            : MiaComponent.Literal(v + "x");
    });
    
    public static ControlValueFormatter Brightness { get; } = Create(v =>
    {
        if (v == 0) return MiaComponent.Translatable("options.gamma.min");
        if (v == 100) return MiaComponent.Translatable("options.gamma.max");
        return MiaComponent.Literal(v + "%");
    });
    
    public static ControlValueFormatter TranslateVariable(string key) =>
        Create(v => MiaComponent.Translatable(key, v));
    
    private static ControlValueFormatter Create(Func<int, IComponent> func) => new(func);
    
    public static IComponent Format(this ControlValueFormatter formatter, int value) => formatter(value);
}