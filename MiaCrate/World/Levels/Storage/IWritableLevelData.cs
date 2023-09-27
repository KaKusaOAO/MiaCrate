using MiaCrate.Core;

namespace MiaCrate.World;

public interface IWritableLevelData : ILevelData
{
    // TODO: Refactor?
    public new int XSpawn { set; }
    public new int YSpawn { set; }
    public new int ZSpawn { set; }
    public new float SpawnAngle { set; }

    public void SetSpawn(BlockPos pos, float angle)
    {
        XSpawn = pos.X;
        YSpawn = pos.Y;
        ZSpawn = pos.Z;
        SpawnAngle = angle;
    }
}