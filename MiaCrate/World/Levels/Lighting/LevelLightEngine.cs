using MiaCrate.Core;

namespace MiaCrate.World;

public class LevelLightEngine : ILightEventListener
{
    public const int LightSectionPadding = 1;
    protected readonly ILevelHeightAccessor _levelHeightAccessor;
    private readonly ILightEngine? _blockEngine;
    private readonly ILightEngine? _skyEngine;

    public bool HasLightWork => _skyEngine?.HasLightWork ?? _blockEngine?.HasLightWork ?? false;

    public LevelLightEngine(ILightChunkGetter level, bool bl, bool bl2)
    {
        _levelHeightAccessor = level.Level;
        _blockEngine = bl ? new BlockLightEngine(level) : null;
        _skyEngine = bl ? new SkyLightEngine(level) : null;
    }

    public void CheckBlock(BlockPos pos)
    {
        _blockEngine?.CheckBlock(pos);
        _skyEngine?.CheckBlock(pos);
    }

    public int RunLightUpdates()
    {
        var i = 0;
        i += _blockEngine?.RunLightUpdates() ?? 0;
        i += _skyEngine?.RunLightUpdates() ?? 0;
        return i;
    }

    public void UpdateSectionStatus(SectionPos pos, bool bl)
    {
        _blockEngine?.UpdateSectionStatus(pos, bl);
        _skyEngine?.UpdateSectionStatus(pos, bl);
    }

    public ILayerLightEventListener GetLayerListener(LightLayer layer)
    {
        return layer == LightLayer.Block
            ? (ILayerLightEventListener?) _blockEngine ?? ILayerLightEventListener.Dummy.Shared
            : (ILayerLightEventListener?) _skyEngine ?? ILayerLightEventListener.Dummy.Shared;
    }
}