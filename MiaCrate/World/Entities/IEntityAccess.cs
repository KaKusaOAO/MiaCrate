using MiaCrate.Core;

namespace MiaCrate.World.Entities;

public interface IEntityAccess
{
    public int Id { get; }
    public Uuid Uuid { get; }
    public BlockPos BlockPosition { get; }
    
    public void SetLevelCallback(IEntityInLevelCallback callback);
}