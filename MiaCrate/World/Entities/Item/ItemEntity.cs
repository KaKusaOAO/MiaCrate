namespace MiaCrate.World.Entities;

public class ItemEntity : Entity
{
    private const int LifeTime = 5 * 60 * SharedConstants.TicksPerSecond;
    private const int InfinitePickupDelay = short.MaxValue;
    private const int InfiniteLifeTime = short.MinValue;
    
    public ItemEntity(IEntityType type, Level level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        Util.LogFoobar();
    }
}