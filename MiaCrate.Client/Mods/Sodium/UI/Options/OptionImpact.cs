using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options;

public sealed class OptionImpact
{
    public static OptionImpact Low { get; } = new(ChatFormatting.Green, "sodium.option_impact.low");
    public static OptionImpact Medium { get; } = new(ChatFormatting.Yellow, "sodium.option_impact.medium");
    public static OptionImpact High { get; } = new(ChatFormatting.Gold, "sodium.option_impact.high");
    public static OptionImpact Varies { get; } = new(ChatFormatting.White, "sodium.option_impact.varies");
    
    public IComponent LocalizedName { get; }

    private OptionImpact(ChatFormatting format, string text)
    {
        LocalizedName = MiaComponent.Translatable(text).WithColor(format.ToTextColor());
    }
}