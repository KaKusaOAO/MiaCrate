using MiaCrate.Core;

namespace MiaCrate.World;

public interface IBlockAndTintGetter : IBlockGetter
{
    public LevelLightEngine LightEngine { get; }
    
    public float GetShade(Direction direction, bool bl);

    public int GetBlockTint(BlockPos pos, ColorResolver resolver);

    public int GetBrightness(LightLayer layer, BlockPos pos) => 
        LightEngine.GetLayerListener(layer).GetLightValue(pos);
}