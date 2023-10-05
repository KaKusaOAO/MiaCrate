using MiaCrate.World.Entities;

namespace MiaCrate.World;

public interface IEntityInLevelCallback
{
    public static IEntityInLevelCallback Null { get; } = new NullInstance();
    
    public void OnMove();
    public void OnRemove(EntityRemovalReason reason);

    private class NullInstance : IEntityInLevelCallback
    {
        public void OnMove() {}

        public void OnRemove(EntityRemovalReason reason) {}
    }
}