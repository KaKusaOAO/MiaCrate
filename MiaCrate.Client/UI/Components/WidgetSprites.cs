namespace MiaCrate.Client.UI;

public record WidgetSprites(ResourceLocation Enabled, ResourceLocation Disabled, ResourceLocation EnabledFocused,
    ResourceLocation DisabledFocused)
{
    public WidgetSprites(ResourceLocation unfocused, ResourceLocation focused)
        : this(unfocused, unfocused, focused, focused) {}
    
    public WidgetSprites(ResourceLocation enabled, ResourceLocation disabled, ResourceLocation enabledFocused)
        : this(enabled, disabled, enabledFocused, disabled) {}
    
    public ResourceLocation Get(bool enabled, bool focused)
    {
        if (enabled)
        {
            return focused ? EnabledFocused : Enabled;
        }

        return focused ? DisabledFocused : Disabled;
    }
}