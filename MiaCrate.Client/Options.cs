using System.Runtime.InteropServices;
using MiaCrate.Data;
using MiaCrate.Texts;
using Mochi.Structs;
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
    
    public OptionInstance<int> FramerateLimit { get; } = new(
        "options.framerateLimit", OptionInstance.NoTooltip<int>(), (label, fps) => fps == 260
            ? GenericValueLabel(label, MiaComponent.Translatable("options.framerateLimit.max"))
            : GenericValueLabel(label, MiaComponent.Translatable("options.framerate", fps)), 
        new OptionInstance.IntRange(10, 260), Codec.IntRange(10, 260), 120,
        i =>
        {
            Game.Instance.LevelRenderer.NeedsUpdate();
        });
    
    public OptionInstance<double> Gamma { get; } = new(
        "options.gamma", OptionInstance.NoTooltip<double>(), (label, gamma) =>
        {
            var val = (int) (gamma * 100);
            switch (val)
            {
                case 0:
                    return GenericValueLabel(label, MiaComponent.Translatable("options.gamma.min"));
                case 50:
                    return GenericValueLabel(label, MiaComponent.Translatable("options.gamma.default"));
                case 100:
                    return GenericValueLabel(label, MiaComponent.Translatable("options.gamma.max"));
                default:
                    return GenericValueLabel(label, val);
            }
        }, OptionInstance.UnitDouble.Instance, 0.5,
        i =>
        {
            Game.Instance.LevelRenderer.NeedsUpdate();
        });
    
    public OptionInstance<int> RenderDistance { get; }
    public OptionInstance<int> SimulationDistance { get; }

    public Options(Game game, string path)
    {
        var bl = Environment.Is64BitOperatingSystem;
        
        RenderDistance = new OptionInstance<int>(
            "options.renderDistance", OptionInstance.NoTooltip<int>(), 
            (label, chunks) => GenericValueLabel(label, MiaComponent.Translatable("options.chunks", chunks)),
            new OptionInstance.IntRange(2, bl 
                ? SharedConstants.MaxRenderDistance 
                : SharedConstants.MaxRenderDistance / 2), bl ? 12 : 8,
            i =>
            {
                Game.Instance.LevelRenderer.NeedsUpdate();
            });
        
        SimulationDistance = new OptionInstance<int>(
            "options.simulationDistance", OptionInstance.NoTooltip<int>(), 
            (label, chunks) => GenericValueLabel(label, MiaComponent.Translatable("options.chunks", chunks)),
            new OptionInstance.IntRange(2, bl
                ? SharedConstants.MaxRenderDistance 
                : SharedConstants.MaxRenderDistance / 2), bl ? 12 : 8,
            i =>
            {
                Game.Instance.LevelRenderer.NeedsUpdate();
            });
    }

    public static IComponent GenericValueLabel(IComponent label, int value) =>
        GenericValueLabel(label, MiaComponent.Literal(value.ToString()));

    public static IComponent GenericValueLabel(IComponent label, IComponent value) => 
        MiaComponent.Translatable("options.generic_value", label, value);

    public void Save()
    {
        
    }
}