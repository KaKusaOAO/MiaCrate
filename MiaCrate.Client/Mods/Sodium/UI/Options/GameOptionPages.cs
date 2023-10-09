using MiaCrate.Client.Platform;
using MiaCrate.Client.Sodium.UI.Options.Controls;
using MiaCrate.Texts;

namespace MiaCrate.Client.Sodium.UI.Options;

public class GameOptionPages
{
    private static readonly SodiumOptionsStorage _sodiumOpts = new();
    private static readonly GameOptionsStorage _vanillaOpts = new();
    
    public static OptionPage General
    {
        get
        {
            var groups = new List<OptionGroup>();
            var is64Bit = Environment.Is64BitOperatingSystem;
            
            groups.Add(OptionGroup.CreateBuilder()
                .Add(OptionImpl<int>.CreateBuilder(_vanillaOpts)
                    .SetName(MiaComponent.Translatable("options.renderDistance"))
                    .SetTooltip(MiaComponent.Translatable("sodium.options.view_distance.tooltip"))
                    .SetControl(o => new SliderControl(o, 2, is64Bit
                        ? SharedConstants.MaxRenderDistance
                        : SharedConstants.MaxRenderDistance / 2, 1, ControlValueFormatters.TranslateVariable("options.chunks")))
                    .SetBinding((o, v) => o.SimulationDistance.Value = v, o => o.RenderDistance.Value)
                    .SetImpact(OptionImpact.High)
                    .SetFlags(OptionFlag.RequiresRendererReload)
                    .Build())
                .Add(OptionImpl<int>.CreateBuilder(_vanillaOpts)
                    .SetName(MiaComponent.Translatable("options.simulationDistance"))
                    .SetTooltip(MiaComponent.Translatable("sodium.options.simulation_distance.tooltip"))
                    .SetControl(o => new SliderControl(o, 2, is64Bit
                        ? SharedConstants.MaxRenderDistance
                        : SharedConstants.MaxRenderDistance / 2, 1, ControlValueFormatters.TranslateVariable("options.chunks")))
                    .SetBinding((o, v) => o.RenderDistance.Value = v, o => o.SimulationDistance.Value)
                    .SetImpact(OptionImpact.High)
                    .SetFlags(OptionFlag.RequiresRendererReload)
                    .Build())
                .Add(OptionImpl<int>.CreateBuilder(_vanillaOpts)
                    .SetName(MiaComponent.Translatable("options.gamma"))
                    .SetTooltip(MiaComponent.Translatable("sodium.options.brightness.tooltip"))
                    .SetControl(o => new SliderControl(o, 0, 100, 1, ControlValueFormatters.Brightness))
                    .SetBinding((o, v) => o.Gamma.Value = v * 0.01, o => (int) (o.Gamma.Value * 100))
                    .Build())
                .Build());
            
            groups.Add(OptionGroup.CreateBuilder()
                .Add(OptionImpl<int>.CreateBuilder(_vanillaOpts)
                    .SetName(MiaComponent.Translatable("options.guiScale"))
                    .SetTooltip(MiaComponent.Translatable("sodium.options.gui_scale.tooltip"))
                    .SetControl(o => new SliderControl(o, 0, Game.Instance.Window.CalculateScale(0, false), 1, ControlValueFormatters.GuiScale))
                    .SetBinding((o, v) => { }, o => 0)
                    .Build())
                .Build());

            return new OptionPage(MiaComponent.Translatable("stat.generalButton"), groups);
        }
    }
}