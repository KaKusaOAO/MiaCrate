using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILightEventListener
{
    public bool HasLightWork { get; }
    public void CheckBlock(BlockPos pos);
    public int RunLightUpdates();
    public void UpdateSectionStatus(SectionPos pos, bool bl);
    
    public void UpdateSectionStatus(BlockPos pos, bool bl) =>
        UpdateSectionStatus(SectionPos.Of(pos), bl);
}