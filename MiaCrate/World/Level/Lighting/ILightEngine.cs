using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILightEngine
{
    
}

public interface ILayerLightEventListener
{
    
}

public interface ILightEventListener
{
    public void CheckBlock(BlockPos pos);
    public bool HasLightWork { get; }
    public int RunLightUpdates();
    public void UpdateSectionStatus(SectionPos pos, bool bl);
    public void UpdateSectionStatus(BlockPos pos, bool bl) =>
        UpdateSectionStatus(SectionPos.Of(pos), bl);
    
    
}