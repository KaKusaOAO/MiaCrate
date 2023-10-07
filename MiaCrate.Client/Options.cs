using MiaCrate.Data;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client;

public class Options
{
    public OptionInstance<bool> Touchscreen { get; } = OptionInstance.CreateBool("options.touchscreen", false);

    public OptionInstance<int> Fov { get; } = new(
        "options.fov", OptionInstance.NoTooltip<int>(), (label, fov) =>
        {
            switch (fov)
            {
                case 70:
                    return GenericValueLabel(label, MiaComponent.Translatable("options.fov.min"));
                case 110:
                    return GenericValueLabel(label, MiaComponent.Translatable("options.fov.max"));
                default:
                    return GenericValueLabel(label, fov);
            }
        }, new OptionInstance.IntRange(30, 110),
        Codec.Double.CrossSelect(d => (int) (d * 40 + 70), i => (i - 70) / 40.0), 70,
        i =>
        {
            Game.Instance.LevelRenderer.NeedsUpdate();
        });

    public Options(Game game, string path)
    {
        
    }

    public static IComponent GenericValueLabel(IComponent label, int value) =>
        GenericValueLabel(label, MiaComponent.Literal(value.ToString()));

    public static IComponent GenericValueLabel(IComponent label, IComponent value) => 
        MiaComponent.Translatable("options.generic_value", label, value);

    public void Save()
    {
        
    }
}