using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILayerLightEventListener : ILightEventListener
{
    public DataLayer? GetDataLayerData(SectionPos pos);
    
    public int GetLightValue(BlockPos pos);
    
    public class Dummy : ILayerLightEventListener
    {
        public static Dummy Shared { get; } = new();
        
        private Dummy() {}

        public bool HasLightWork => false;

        public DataLayer? GetDataLayerData(SectionPos pos) => null;
        public int GetLightValue(BlockPos pos) => 0;
        public void CheckBlock(BlockPos pos) {}
        public int RunLightUpdates() => 0;
        public void UpdateSectionStatus(SectionPos pos, bool bl) {}
    }
}